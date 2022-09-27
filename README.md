# Project Lighthouse

[![Continuous Integration](https://github.com/LBPUnion/ProjectLighthouse/actions/workflows/ci.yml/badge.svg)](https://github.com/LBPUnion/ProjectLighthouse/actions/workflows/ci.yml)
![GitHub commit activity](https://img.shields.io/github/commit-activity/m/LBPUnion/ProjectLighthouse)
![GitHub contributors](https://img.shields.io/github/contributors/LBPUnion/ProjectLighthouse)
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/LBPUnion/ProjectLighthouse)
![Source Lines of Code](https://tokei.ekzhang.com/b1/github/LBPUnion/ProjectLighthouse)

Project Lighthouse is a clean-room, open-source custom server for LittleBigPlanet. This is a project conducted by
the [LBP Union Ministry of Technology Research and Development team](https://www.lbpunion.com/technology).

For concerns and inquiries about the project, please contact us [here](https://www.lbpunion.com/contact).

For general questions and discussion about Project Lighthouse, please see
the [megathread](https://www.lbpunion.com/forum/union-hall/project-lighthouse-littlebigplanet-private-servers-megathread)
on our forum.

## DISCLAIMERS (Please read!)

### This is not a final product.
This is **beta software**, and thus is **not stable nor is it secure**.

While Project Lighthouse is in a mostly working state, **we ask that our software not be used in a production
environment until release**.

This is because we have not entirely nailed security down yet, and **your instance WILL get attacked** as a result. It's
happened before, and it'll happen again.

Simply put, **Project Lighthouse is not ready for the public yet**.

In addition, we're not responsible if someone hacks your machine and wipes your database, so make frequent backups, and
be sure to report any vulnerabilities. Thank you in advance.

### We are not obligated to provide support.

Project Lighthouse is open source. However, it is licensed under the GNU Affero General Public License version 3 (
AGPLv3)
meaning that Project Lighthouse is provided to you as-is, with **absolutely no warranty.**

Please understand that while this license gives you freedom to do pretty much anything you would want to do, including
allowing you to run your instance,
**this doesn't mean we are obligated to support you or your instance**. When you set up an instance of Project
Lighthouse, you are entirely on your own.

### Sony is not related nor liable.

[//]: # (Referenced from https://www.lbpunion.com/post/project-lighthouse-littlebigplanet-private-servers)

It is very important to stress that the LBP Union and Project Lighthouse is not affiliated with Sony Group
Corporation *(collectively referred to as “Sony”)* and its subordinate entities and studios. We are not the official
developers of LittleBigPlanet or it's online services. Project Lighthouse is a clean-room reimplementation of its
server, not the official servers.

By using Project Lighthouse you release Sony, as well as any employees or agents of Sony, from any and all liability,
corporate, or personal loss caused to you or others by the use of Project Lighthouse or any features we provide.

## Building

This will be written when we're out of beta. Consider this your barrier to entry ;).

It is recommended to build with `Release` if you plan to use Lighthouse in a production environment.

## Contributing

Please see [`CONTRIBUTING.md`](https://github.com/LBPUnion/ProjectLighthouse/blob/main/CONTRIBUTING.md) for more
information.

## Compatibility across games and platforms

| Game     | Console (PS3/Vita/PSP) | Emulator (RPCS3/Vita3k/PPSSPP)            | Next-Gen (PS4/PS5/Adrenaline) |
|----------|------------------------|-------------------------------------------|-------------------------------|
| LBP1     | Compatible             | Compatible                                | No next-gen equivalent        |
| LBP2     | Compatible             | Compatible                                | No next-gen equivalent        |
| LBP3     | Compatible             | Compatible                                | Incompatible                  |
| LBP Vita | Compatible             | Incompatible, PSN not supported on Vita3k | No next-gen equivalent        |
| LBP PSP  | Potentially compatible | Incompatible, PSN not supported on PPSSPP | Potentially Compatible        |

Project Lighthouse is mostly a work in progress, so this chart is subject to change at any point.
