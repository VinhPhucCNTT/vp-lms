# Backend tests

Run these commands from the repository root with the .NET 10 SDK. Unit tests run without PostgreSQL.

```powershell
dotnet test Backend/Backend.Api.Tests/Backend.Api.Tests.csproj --filter "Category!=Integration"
```

## One-time PostgreSQL setup

Open `psql` as an administrator on a local PostgreSQL instance. A separate local instance or container is also suitable. Run these statements outside a transaction and choose a unique password at the prompt.

```sql
CREATE ROLE vp_lms_integration_runner LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOREPLICATION NOBYPASSRLS;
\password vp_lms_integration_runner
CREATE DATABASE vp_lms_integration_tests OWNER vp_lms_integration_runner;
REVOKE ALL ON DATABASE vp_lms_integration_tests FROM PUBLIC;
```

`\password` is a `psql` command. Keep this role dedicated to the test database. Do not grant it membership in other roles or privileges on development or production objects. If the instance uses custom authentication rules, allow this role to connect only to the test database over loopback.

In PowerShell 7, enter the complete connection string at a masked prompt so it stays out of command history. Use the test password and adjust the port as needed.

```text
Host=127.0.0.1;Port=5432;Database=vp_lms_integration_tests;Username=vp_lms_integration_runner;Password=<test password>;Timeout=5
```

```powershell
$env:VP_LMS_TEST_DB = Read-Host "Dedicated PostgreSQL test connection string" -MaskInput

# PostgreSQL integration tests
dotnet test Backend/Backend.Api.Tests/Backend.Api.Tests.csproj --filter "Category=Integration"

# All backend tests
dotnet test Backend/Backend.Api.Tests/Backend.Api.Tests.csproj

Remove-Item Env:VP_LMS_TEST_DB
```

Use a quoted password value in the connection string if it contains semicolons. The environment variable is visible to processes inheriting that shell environment. Avoid storing it in checked-in files or shared shell profiles.

The test host validates the target before it starts and checks the connection resolved by the API before applying the existing EF migrations. Use an empty database for the first run. The tests own and clean up only their fixture records. Use one test process at a time per database.

If the integration suite fails during setup, check that `VP_LMS_TEST_DB` is set in the shell running the tests. It must explicitly name the database and role above, use a single loopback host, and omit connection options. If the server is unreachable or rejects login, confirm the port and credentials by running `psql -h 127.0.0.1 -p 5432 -U vp_lms_integration_runner -d vp_lms_integration_tests -W`. A selected integration suite fails if setup is unavailable.
