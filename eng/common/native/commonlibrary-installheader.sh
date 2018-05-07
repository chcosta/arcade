#!/usr/bin/env bash

source="${BASH_SOURCE[0]}"

# resolve $source until the file is no longer a symlink
while [[ -h "$source" ]]; do
    scriptroot="$( cd -P "$( dirname "$source" )" && pwd )"
    source="$(readlink "$source")"
    # if $source was a relative symlink, we need to resolve it relative to the path where the
    # symlink file was located
    [[ $source != /* ]] && source="$scriptroot/$source"
done
scriptroot="$( cd -P "$( dirname "$source" )" && pwd )"

installpath=''
baseuri=''
toolversion=''
commonlibrarydirectory="$scriptroot"
force=''

while (($# > 0)); do
    lowerI="$(echo $1 | awk '{print tolower($0)}')"
    case $lowerI in
      --installpath)
        installpath=$2
        shift 1
        ;;
      --baseuri)
        baseuri=$2
        shift 1
        ;;
      --version)
        toolversion=$2
        shift 1
        ;;
      --commonlibrarydirectory)
        commonlibrarydirectory=$2
        shift 1
        ;;
      --force)
        force=$1
        ;;
      *)
        unknownproperties="$unknownproperties $1"
        ;;
    esac
    shift 1
done

if [[ $unknownproperties != '' ]]; then
    echo_error "Argument(s) $unknownproperties are not recognized"
    exit 1
fi

# Import common library
source "$commonlibrarydirectory/commonlibrary.sh"
