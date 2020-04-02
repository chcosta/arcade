# Signing Roadmap

Signing plan documentation is available [here](https://github.com/dotnet/arcade/blob/master/Documentation/Projects/Signing/SigningPlan.md)

## Stage 1 - early April

- Create a build manifest

- Update signing validation to work with build manifest

## Stage 2: MSI signing - mid May

- Create task / target for preserving Wix and /or MSI obj/metadata - late April

- Promotion job which will consume build manifest, artifacts, and obj data to construct a valid MSI, and publish to signed feed - mid May

## Stage 3: Other signing - early June

- Enable package signing in Promotion job

- Enable RPM / deb / etc in Promotion job

- Add Mac notarization to Promotion job

## Stage 4: Transition - June - July

- Switch repos to consume / publish to unsigned feed and publish to signed feed

- Up until this point, signing continues as it currently is.  During this stage, we will being switching some repos to only sign via promotion.
