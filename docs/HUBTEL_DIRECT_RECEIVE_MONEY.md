# Direct Receive Money

## Overview

The **Hubtel Sales API** allows you to sell goods and services online, in-store, and on mobile. With a single integration, you can:

1. Accept mobile money payments on your application, sell services in-store, online, and on mobile, as well as process all your sales on your Hubtel account.
2. Send money to your customers.

This API can be used to provide a range of services including:
- E-commerce payment processing
- Mobile banking
- Bulk payments
- Accepting payments for goods and services into your Hubtel account

The following sections provide an overview of the **Receive Money API** endpoints for interacting programmatically within your application.

## ⚠️ Note

Businesses are required to implement a security feature for this API in order to prevent individuals from receiving unsolicited prompts.

This security feature may be:

1. Only registered users can make payments, and before payment initiation, they cannot edit their numbers.  
   **OR**
2. Send OTP to unregistered users to confirm the numbers before proceeding to initiate payment.

---

## Available Channels

The following are the available channels through which a merchant can **receive mobile money** into a Hubtel Merchant Account.
| Mobile Money Provider | Channel Name |
|----------------------|--------------|
| MTN Ghana            | `mtn-gh`     |
| Telecel Ghana        | `vodafone-gh`|
| AirtelTigo Ghana     | `tigo-gh`    |


## Getting Started

### Business IP Whitelisting

You must share your public IP address with your **Retail System Engineer** for whitelisting.

> **💡 Note**  
> All API endpoints are live, and **only requests from whitelisted IP address(es)** can reach the endpoints shared in this reference.  
>  
> Requests from IP addresses that have not been whitelisted will return a **403 Forbidden** error response or may time out.  
>  
> A maximum of **4 IP addresses per service** is permitted.

---

## Understanding the Service Flow

The **Hubtel Sales API** allows you to integrate multiple functionalities into your applications. This document focuses on the following APIs:

- **Direct Receive Money API**  
  REST API to receive money directly into your Hubtel Merchant Account from a Mobile Money wallet across all available networks.

- **Transaction Status Check API**  
  REST API to check the status of any debit transaction initiated **five (5) or more minutes** after the completion of the debit transaction.  
  It is **mandatory** to implement this API **only for transactions where a callback is not received from Hubtel**.

The entire process is **asynchronous**.  
The figure below demonstrates the service flow using these two endpoints.


![Direct Receive Money Flow](images/Direct_Receive_Money.png)


The table below describes the steps involved in receiving money using the API:

## Receive Money – Service Steps

| Step | Description |
|------|-------------|
| 1 | Client App makes a **Receive Money** request to Hubtel. |
| 2 | Hubtel performs authentication on the request and sends a response to the Client App accordingly. |
| 3 | A final callback is sent to the Client App via the **PrimaryCallbackURL** provided in the request. |
| 4 | If the merchant does not receive the final status of the transaction **after five (5) minutes**, it is **mandatory** to perform a status check using the **Transaction Status Check API** to determine the final status of the transaction. |

---

## API Reference

**Direct Receive Money** allows you to accept direct mobile money payments into your Hubtel Merchant Account.

> ⚠️ **Important**  
> The flow for charging mobile subscribers differs across various Telco networks.

To initiate a Receive Money transaction, send an **HTTP POST** request to the endpoint below with the required parameters.

It is **mandatory** to pass your **POS Sales ID** for Receive Money requests.  
You can find your POS Sales ID from your Hubtel dashboard.

---

### Endpoint Details

| Property | Value |
|--------|-------|
| **API Endpoint** | `https://rmp.hubtel.com/merchantaccount/merchants/{POS_Sales_ID}/receive/mobilemoney` |
| **Request Type** | `POST` |
| **Content Type** | `application/json` |

---

## Request Parameters

| Parameter | Type | Requirement | Description |
|---------|------|------------|-------------|
| `CustomerName` | String | Optional | The name on the customer’s mobile money wallet. |
| `CustomerMsisdn` | String | **Mandatory** | The customer’s mobile money number in international format. Example: `"23324911141"` |
| `CustomerEmail` | String | Optional | Email address of the customer. |
| `Channel` | String | **Mandatory** | The mobile money channel provider. Available values: `mtn-gh`, `vodafone-gh`, `tigo-gh` |
| `Amount` | Float | **Mandatory** | Amount to be debited. Only **2 decimal places** allowed. Example: `0.50` |
| `PrimaryCallbackURL` | String | **Mandatory** | URL used to receive callback payloads for Receive Money transactions from Hubtel. |
| `Description` | String | **Mandatory** | A brief description of the transaction. |
| `ClientReference` | String | **Mandatory** | A **unique** reference provided by the API user for every transaction. Must be alphanumeric and **maximum length is 36 characters**. |

---

> **💡 Note**  
> The `ClientReference` value **must never be duplicated** for any transaction.
>
> ## Sample Request

```http
POST /merchantaccount/merchants/11684/receive/mobilemoney HTTP/1.1
Host: rmp.hubtel.com
Accept: application/json
Content-Type: application/json
Authorization: Basic endjeOBiZHhza250fT3=
Cache-Control: no-cache
{
  "CustomerName": "Joe Doe",
  "CustomerMsisdn": "233200010000",
  "CustomerEmail": "username@example.com",
  "Channel": "vodafone-gh",
  "Amount": 0.8,
  "PrimaryCallbackUrl": "https://webhook.site/b503d1a9-e726-f315254a6ede",
  "Description": "Union Dues",
  "ClientReference": "3jL2KlUy3vt21"
}

```
## Response Parameters

| Parameter | Type | Description |
|---------|------|-------------|
| `Message` | String | Description of the response received from the Receive Money API related to the `ResponseCode`. |
| `ResponseCode` | String | The unique response code indicating the status of the transaction. |
| `Data` | Object | An object containing the required data returned from the API. |
| `Amount` | Float | The transaction amount. |
| `Charges` | Float | The charge/fee applied to the transaction. |
| `AmountAfterCharges` | Float | The transaction amount after charges/fees have been deducted. |
| `AmountCharged` | Float | The actual amount charged from the customer’s mobile money wallet. |
| `Description` | String | The description initially provided by the API user in the request payload. |
| `ClientReference` | String | The reference ID initially provided by the client/API user in the request payload (**from merchant**). |
| `TransactionId` | String | The unique ID used to identify a Hubtel transaction (**from Hubtel**). |
| `ExternalTransactionId` | String | The transaction reference from the mobile money provider (**from Telco**). |
| `OrderID` | String | The unique ID used to identify a transaction order. |
| `DeliveryFee` | Float | The delivery fee for the transaction (normally `0.0`). |

 ## Sample Response

```json
{
  "Message": "Transaction pending. Expect callback request for final state",
  "ResponseCode": "0001",
  "Data": {
    "TransactionId": "09f84e20a283942e807128e8c21d08d6",
    "Description": "Union Dues",
    "ClientReference": "3jL2KlUy3vt21",
    "Amount": 0.8,
    "Charges": 0.05,
    "AmountAfterCharges": 0.8,
    "AmountCharged": 0.85,
    "DeliveryFee": 0.0
  }
}
```

## Receive Money Callback

The **Hubtel Receive Money API** mandatorily sends a payload to the `callbackURL` provided in each request.

The callback payload determines the **final status** of a pending transaction response (i.e. transactions with `ResponseCode = 0001`).

The callback URL specified in the Receive Money request **must be implemented to accept an HTTP POST payload** from Hubtel.

To process a Receive Money request, customer approval is required to transfer money from their mobile money account to your Hubtel Merchant Account. As a result, the transaction flow is **asynchronous**.

The final transaction status may take up to **30 seconds** to be confirmed.  
Therefore, it is **crucial** to implement an HTTP callback on your server to receive the final transaction status.

---

### Sample Callback (Successful)

```json
{
  "ResponseCode": "0000",
  "Message": "success",
  "Data": {
    "Amount": 0.8,
    "Charges": 0.05,
    "AmountAfterCharges": 0.8,
    "Description": "The Vodafone Cash payment has been approved and processed successfully",
    "ClientReference": "3jL2KlUy3vt21",
    "TransactionId": "09f84e20a283942e807128e8c21d08d6",
    "ExternalTransactionId": "2116938399",
    "AmountCharged": 0.85,
    "OrderId": "09f84e20a283942e807128e8c21d08d6",
    "PaymentDate": "2024-05-14T00:44:57.5142719Z"
  }
}
```

Sample Callback (Failed)
```json
{
  "ResponseCode": "2001",
  "Message": "failed",
  "Data": {
    "Amount": 0.8,
    "Charges": 0.05,
    "AmountAfterCharges": 0.8,
    "Description": "FAILED",
    "ClientReference": "3jL2KlUy3vt21",
    "TransactionId": "09f84e20a283942e807128e8c21d08d6",
    "ExternalTransactionId": "2116938399",
    "AmountCharged": 0.85,
    "OrderId": "09f84e20a283942e807128e8c21d08d6",
    "PaymentDate": "2024-05-14T00:44:57.5142719Z"
  }
}
```

## Transaction Status Check

It is **mandatory** to implement the **Transaction Status Check API** as it allows merchants to check the status of a transaction in rare instances where the merchant does not receive the final status of the transaction from Hubtel **after five (5) minutes**.

To check the status of a transaction, send an **HTTP GET** request to the URL below with **one or more unique transaction identifiers** as parameters.

It is also **mandatory** to pass your **POS Sales ID** for Status Check requests in the endpoint.  
You can find your POS Sales ID in your Hubtel dashboard.

> **💡 Note**  
> Only requests from **whitelisted IP address(es)** can reach the endpoint.  
> Requests from IP addresses that have not been whitelisted will return a **403 Forbidden** error response or may time out.  
> Submit your public IP address(es) to your **Retail Systems Engineer** to be whitelisted.

---

### Endpoint Details

| Property | Value |
|--------|------|
| **API Endpoint** | `https://api-txnstatus.hubtel.com/transactions/{POS_Sales_ID}/status` |
| **Request Type** | `GET` |
| **Content Type** | `application/json` |

---

## Request Parameters

| Parameter | Type | Requirement | Description |
|---------|------|------------|-------------|
| `clientReference` | String | **Mandatory (preferred)** | The client reference of the transaction specified in the request payload. |
| `hubtelTransactionId` | String | Optional | Transaction ID from Hubtel after successful payment. |
| `networkTransactionId` | String | Optional | The transaction reference from the mobile money provider. |

> **💡 Note**  
> Although any one of the unique transaction identifiers above can be passed as parameters, **`clientReference` is recommended** to be used most often.

## Sample Request

```http
GET /transactions/11684/status?clientReference=fhwrthrthejhjmt HTTP/1.1
Host: api-txnstatus.hubtel.com
Authorization: Basic QmdfaWghe2Jhc2U2NF9lbmNvZGUoa2hzcW9seXU6bXVhaHdpYW8pfQ==
```
## Response Parameters

| Parameter | Type | Description |
|---------|------|-------------|
| `message` | String | Description of the response received from the API related to the `ResponseCode`. |
| `responseCode` | String | The response code returned by the API after the request. |
| `data` | Object | An object containing the required data response from the API. |
| `date` | String | Date of the transaction. |
| `status` | String | Status of the transaction (e.g. **Paid**, **Unpaid**, or **Refunded**). |
| `transactionId` | String | The unique ID used to identify a Hubtel transaction (**from Hubtel**). |
| `externalTransactionId` | String | The transaction reference from the mobile money provider (**from Telco**). |
| `paymentMethod` | String | The mode of payment. |
| `clientReference` | String | The reference ID initially provided by the client/API user in the request payload (**from merchant**). |
| `currencyCode` | String | Currency of the transaction (may be `null`). |
| `amount` | Float | The transaction amount. |
| `charges` | Float | The charge/fee applied to the transaction. |
| `amountAfterCharges` | Float | The transaction amount after charges/fees deduction. |
| `isFulfilled` | Boolean | Indicates whether the service was fulfilled (may be `null`). |

## Sample Response (Paid)

```json
{
  "message": "Successful",
  "responseCode": "0000",
  "data": {
    "date": "2024-04-25T21:45:48.4740964Z",
    "status": "Paid",
    "transactionId": "7fd01221faeb41469daec7b3561bddc5",
    "externalTransactionId": "0000006824852622",
    "paymentMethod": "mobilemoney",
    "clientReference": "1sc2rc8nwmchngs9ds2f1dmn",
    "currencyCode": null,
    "amount": 0.1,
    "charges": 0.02,
    "amountAfterCharges": 0.08,
    "isFulfilled": null
  }
}
```


## Sample Response (Unpaid)
```json
{
  "message": "Successful",
  "responseCode": "0000",
  "data": {
    "date": "2024-04-25T21:45:48.4740964Z",
    "status": "Unpaid",
    "transactionId": "7fd01221faeb41469daec7b3561bddc5",
    "externalTransactionId": "0000006824852622",
    "paymentMethod": "mobilemoney",
    "clientReference": "1sc2rc8nwmchngs9ds2f1dmn",
    "currencyCode": null,
    "amount": 0.1,
    "charges": 0.02,
    "amountAfterCharges": 0.08,
    "isFulfilled": null
  }
}
```
## Response Codes

The **Hubtel Sales API** uses standard HTTP error reporting.

- Successful requests return HTTP status codes in the **2xx** range.
- Failed requests return HTTP status codes in the **4xx** or **5xx** range.

In addition to the HTTP status code, **Hubtel-specific response codes** are included in the JSON response body. These response codes provide additional information about the outcome of the request.

## Response Code Reference

| Response Code | Description | Required Action |
|--------------|-------------|-----------------|
| `0000` | The transaction has been processed successfully. | None |
| `0001` | Request has been accepted. A callback will be sent on final state. | None |
| `2001` | Possible causes include:<br>- MTN Mobile Money user has reached counter or balance limits<br>- Insufficient funds or missing permissions<br>- Vodafone Cash transaction failed<br>- Wrong PIN entered<br>- Insufficient funds (multiple invalid attempts may lock account)<br>- MTN exception: Account holder with FRN not found<br>- Transaction ID is invalid | - Customer entered no or invalid PIN<br>- Mobile network unable to parse request<br>- USSD session timeout<br>- Invalid characters (e.g. `&`, `%`, `^`) in description<br>- Ensure provided number matches the selected channel |
| `4000` | Validation errors. | Validation errors detected. Please check the request payload and try again. |
| `4070` | Unable to complete payment at the moment. Fees not set for given conditions. | Ensure the required minimum amount is passed or contact Hubtel Relationship Manager to configure fees. |
| `4101` | Possible causes include:<br>- Business not fully set up to receive payments<br>- API keys missing `mobilemoney-receive-direct` scope<br>- Client request keys do not match business API keys | - Contact your Retail Systems Engineer to enable Receive Money scopes<br>- Ensure correct **Basic Auth** key is used<br>- Ensure your **POS Sales ID** is correctly passed in the endpoint |
| `4103` | Permission denied. Account is not allowed to transact on this channel. | Contact your Retail Systems Engineer |
