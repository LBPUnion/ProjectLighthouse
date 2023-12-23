
# Errors

Here's a list of error codes, as well as their explanations and potential fixes, that are displayed within in-game and
website notifications to indicate what went wrong.

## Level Publishing

- `LH-PUB-0001`: The level failed to publish because the slot is null.
  - **Note:** The slot name will not be displayed in the notification if this error occurs.
- `LH-PUB-0002`: The level failed to publish because the slot does not include a `rootLevel`.
- `LH-PUB-0003`: The level failed to publish because the resource list is null.
- `LH-PUB-0004`: The level failed to publish because the level name is too long.
  - **Fix:** Shorten the level name to something below 64 characters.
- `LH-PUB-0005`: The level failed to publish because the level description is too long.
  - **Fix:** Shorten the level description to something below 512 characters.
- `LH-PUB-0006`: The level failed to publish because the server is missing resources required by the level.
  - **Potential Fix:** Remove any resources that are not available on the server from the level.
- `LH-PUB-0007`: The level failed to publish because the root level is not a valid level.
- `LH-PUB-0008`: The level failed to publish because the root level is not an LBP3 Adventure level.
- `LH-PUB-0009`: The level failed to publish because the the user has reached their level publishing limit.
  - **Fix:** Delete some of your previously published levels to make room for new ones.
- `LH-PUB-0010`: THe level failed to publish because the icon of the level is not a valid texture or image.

## Level Republishing

- `LH-REP-0001`: The level failed to republish because the old slot does not exist.
- `LH-REP-0002`: The level failed to republish because the original publisher is not the current publisher.
  - **Potential Fix:** Copying the level to another slot on your moon typically fixes this issue.
- `LH-REP-0003`: The level could not be unlocked because it was locked by a moderator.
  - **Potential Fix:** Ask a server administrator/moderator to unlock the level for you.