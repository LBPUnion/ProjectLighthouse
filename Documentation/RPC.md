
# RPC (Rich Presence)

Lighthouse supports Discord's [Rich Presence](https://discordapp.com/rich-presence) feature, which allows you
to display your current activity in Lighthouse to your Discord friends.

This can be done by utilizing a pre-existing RPC client, such as [this one](https://github.com/LBPUnion/PLRPC),
or by creating your own. This page will explain how to utilize the `/api/v1/rpc` endpoint to access the
required information.

## Authentication

This endpoint does not require any form of authentication and is available to use without restriction, aside
from any server admin imposed rate limiting.

## Endpoints

### GET `/api/v1/rpc`

Returns a JSON object containing the following information:

- `applicationId`: The ID of the Discord application.
- `partyIdPrefix`: The prefix to use for the party ID. This is used to prevent collisions between
  multiple instances of Lighthouse running on the same Discord application.
- `usernameType`: Some compatible APIs require usernames instead of user IDs. A return value of `0`
  indicates that user IDs should be used, while a return value of `1` indicates that usernames should be used.
- `assets`: A JSON object containing the following information:
    - `podAsset`: Asset used when in the Pod.
    - `moonAsset`: Asset used when creating on the Moon.
    - `remoteMoonAsset`: Asset used when creating on a remote Moon.
    - `developerAsset`: Asset used when playing a story mode level.
    - `developerAdventureAsset`: Asset used when playing an adventure level.
    - `dlcAsset`: Asset used when playing a DLC level.
    - `fallbackAsset`: Asset used when client can't determine the slot type.

> **Warning**
> All `assets` properties are nullable and will return `null` if not set in configuration. Be sure to account
> for this when using the returned data.