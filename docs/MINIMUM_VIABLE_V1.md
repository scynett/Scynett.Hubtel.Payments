# Minimum Viable v1.0 Checklist

## ?? BLOCKERS (Must Fix Before Any Release)

- [ ] **Verify API Endpoint** (2 hours)
  - Check Hubtel docs for `/receive/mobilemoney`
  - Verify base URL is `https://api.hubtel.com`
  - Confirm request/response format matches

- [ ] **Add Refit Exception Handling** (4 hours)
  - Update `ReceiveMobileMoneyService.cs`
  - Catch `ApiException` from Refit
  - Parse Hubtel error responses
  - Return proper `Result.Failure`

- [ ] **Test Against Real API** (1 day)
  - Get Hubtel sandbox credentials
  - Make actual API call
  - Verify it works end-to-end
  - Document any issues found

## ?? HIGH PRIORITY (Strongly Recommended)

- [ ] **10 Basic Unit Tests** (1 day)
  - Test `HubtelResponseDecisionFactory` (all codes)
  - Test `Result<T>.Success()`
  - Test `Result<T>.Failure()`
  - Test resilience config loads
  - Test pending store add/remove

- [ ] **Update README** (1 hour)
  - Add "Preview Release" warning
  - Add "Tested Against" section
  - Add "Known Limitations"
  - Add "Report Issues" link

## ? NICE TO HAVE (Can Wait for v1.1)

- [ ] Input validation (FluentValidation)
- [ ] Webhook signature validation
- [ ] Health checks
- [ ] Configuration validation
- [ ] Integration tests
- [ ] Sample application
- [ ] Idempotency support

## ?? Release Checklist

Once above blockers are fixed:

- [ ] Update version to `1.0.0-preview1`
- [ ] Update CHANGELOG.md
- [ ] Create GitHub release
- [ ] Publish to NuGet.org
- [ ] Monitor for issues
- [ ] Gather feedback

## ?? Total Estimated Time

- **Blockers**: 1.5 days
- **High Priority**: 2 days
- **Total for v1.0.0-preview1**: 3-4 days

## ?? You Can Ship This Week!

**Monday-Tuesday**: Fix blockers + add exception handling  
**Wednesday-Thursday**: Write basic tests  
**Friday**: Package, test, and publish preview  

**Then gather feedback for v1.0.0 final in 2-3 weeks.**
