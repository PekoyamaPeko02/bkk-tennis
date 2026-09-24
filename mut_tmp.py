import subprocess, sys
R = '/Users/liberix_dev/Github/BKK_Tennis/'
A = R + 'backend/src/BkkTennis.Api/Features/Auth/AuthEndpoints.cs'
muts = [
 ('drop eligible role check', A, 'var eligible = user is { PasswordHash: not null } && user.Role is Roles.PartnerStaff or Roles.PartnerOwner;', 'var eligible = user is { PasswordHash: not null };', 'be'),
 ('skip lock check', A, 'var locked = user?.LockedUntil > now;', 'var locked = false;', 'be'),
 ('lock at 3', A, 'private const int MaxFailedLogins = 5;', 'private const int MaxFailedLogins = 3;', 'be'),
 ('no reset on success', A, 'if (user!.FailedLogins != 0 || user.LockedUntil is not null)', 'if (false)', 'be'),
 ('401 instead of 400', A, 'return Results.BadRequest(new { code = "invalid_credentials"', 'return Results.Json(statusCode: 401, data: new { code = "invalid_credentials"', 'be'),
 ('remove rate limiter on staff login', A, '        }).RequireRateLimiting("otp").WithValidation();\n\n        g.MapGet("/me"', '        }).WithValidation();\n\n        g.MapGet("/me"', 'be'),
 ('remove ToLowerInvariant', A, 'req.Email.Trim().ToLowerInvariant()', 'req.Email.Trim()', 'be'),
]
def run(cmd):
    return subprocess.run(cmd, shell=True, cwd=R, capture_output=True, text=True)
def stat():
    return run('git diff --stat | tail -1').stdout.strip()
base = stat(); print('baseline', base)
which = sys.argv[1]
if which == 'be':
    for name, f, old, new, _ in muts:
        s = open(f).read()
        if old not in s:
            print(name, 'PATTERN NOT FOUND'); continue
        open(f, 'w').write(s.replace(old, new, 1))
        r = run('dotnet test backend/tests/BkkTennis.IntegrationTests --filter "StaffLoginTests|ClubOsAuthorizationTests" 2>&1 | grep -E "^\\s+Failed |Passed!|Failed!|error" | head -6')
        open(f, 'w').write(s)
        print('==', name, '->', r.stdout.strip().replace('\n', ' | '))
        print('   diff ok:', stat() == base)
