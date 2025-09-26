# Security Policy

## Supported Versions

We only provide security fixes and support for versions listed below.  
Older releases are **end-of-life** and will not receive patches.

| Version range  | Supported |
|:--------------:| :---: |
|    >= 1.1.7    | ✅ |
|    < 1.1.7     | ❌ |

> Note: We use [Semantic Versioning](https://semver.org/). When reporting, please include your exact version (e.g., `1.2.3`), how you installed the project, and your runtime/OS details.

---

## Reporting a Vulnerability

**Please do not open a public issue for security problems.**

- **Primary contact:** info@protforce.de  
- **GitHub Security Advisory:** You can also report via **GitHub → Security → Advisories → Report a vulnerability** on this repository.

We aim to acknowledge new reports **within 5 business days** and provide a timeline for remediation after initial triage.

Please include:
- A clear description of the issue and impact
- Steps to reproduce (PoC preferred)
- Affected supported version(s)
- Environment details (OS, runtime, config)
- Any known workarounds

---

## Disclosure & Embargo Policy

- We follow **coordinated disclosure**.
- Typical timeline:
  1. **Acknowledge** report (≤ 5 business days)
  2. **Triage & assess severity** using CVSS v3.1 (≤ 7 days)
  3. **Fix development & testing** (timeline depends on severity/complexity)
  4. **Release** patched versions for supported lines
  5. **Public advisory** after a fixed version is available
- We may request an **embargo** for high-impact issues until patches are broadly available.

---

## Security Update Process

- Fixes land on the **default branch**.
- We publish a **release note** and **GitHub Security Advisory** with:
  - Impact and severity (CVSS score)
  - Affected versions and fixed versions
  - Mitigations/workarounds (if any)
  - Credits to reporters (opt-in)

---

## Scope

Security issues include (non-exhaustive):
- Remote code execution, privilege escalation, authz/authn bypass
- Sensitive data exposure, injection, sandbox escape
- Protocol/crypto misuse within the project
- Supply-chain issues involving this repository’s released artifacts

**Out of scope** (examples):

- Vulnerabilities in **unsupported versions**
- Issues in third-party dependencies without a demonstrable impact here
- Best-practice or configuration hardening requests without a concrete vuln
- DoS via unrealistic limits unless affecting default safe use

---

## Dependencies & Supply Chain

- If a vulnerability is due to a dependency:
  - We will **update/patch** the dependency and cut a release for supported versions.
  - If no upstream fix exists, we may publish **temporary mitigations**.

---

## Responsible Use & Hardening

- Run with least privileges and follow the hardening steps in our docs.
- Avoid exposing admin interfaces to untrusted networks.
- Rotate keys/credentials regularly and keep your environment up-to-date.

---

## Contact & Legal

- Security contact: info@protforce.de  
- Please comply with applicable laws. We will not pursue legal action for **good-faith** research conducted under this policy.
