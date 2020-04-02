# Signing Roadmap

Signing plan documentation is available [here](https://github.com/dotnet/arcade/blob/master/Documentation/Projects/Signing/SigningPlan.md)

## Stage 1 - early April

- Create a build manifest

- Update signing validation to work with build manifest

- Enable not signing non-shipping packages

## Stage 2: MSI signing - late April

- Create task / target for preserving MSI obj/metadata - mid April

- Promotion job which will consume build manifest, artifacts, and obj data to construct a valid MSI, and publish to signed feed

## Stage 3: Other signing - late May

- Enable package signing in Promotion job

- Enable RPM / deb / etc in Promotion job

- Add Mac notarization to Promotion job

## Stage 4: Transition - June

- Switch repos to consume / publish to unsigned feed and publish to signed feed

- Up until this point, signing continues as it currently is.  During this stage, we will being switching some repos to only sign via promotion.
