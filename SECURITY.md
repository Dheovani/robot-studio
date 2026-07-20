# Security Policy

## Supported Versions

RobotStudio is in early development. Only the current main development line is
considered for security review.

No production deployment, hosted service, hardware integration, or public API is
currently supported.

## Reporting Security Issues

If you discover a security issue, report it privately to the project owner
through the contact channel listed in the repository, if one is available.

Please include:

- a clear description of the issue;
- steps to reproduce it;
- affected files, commands, or inputs;
- possible impact;
- any suggested fix, if known.

Do not publicly disclose a security issue before the project owner has had a
reasonable opportunity to review it.

## Scope

Security reports may cover:

- unsafe file handling;
- command execution risks;
- dependency or build-chain concerns;
- parser inputs that can crash or corrupt execution;
- future hardware or communication risks once those modules exist.

The following are currently out of scope:

- production uptime;
- hosted-service vulnerabilities;
- account or authentication issues;
- hardware firmware vulnerabilities;
- third-party deployments not controlled by the project owner.

## License And Use Restrictions

Security review or disclosure does not grant additional license rights. The
project remains proprietary and limited to personal, non-commercial study use
unless the copyright holder gives prior written permission.
