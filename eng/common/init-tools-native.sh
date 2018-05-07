#!/usr/bin/env bash

# Stop script on NZEC
set -e
# Stop script if unbound variable found (use ${var:-} if intentional)
set -u
# By default cmd1 | cmd2 returns exit code of cmd2 regardless of cmd1 success
# This is causing it to fail
set -o pipefail

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

baseuri='https://dotnetfeed.blob.core.windows.net/chcosta-test/nativeassets'
toolsversionsfile="$scriptroot/../NativeToolsVersions.txt"
clean=false
force=''
COMMONLIBRARY_DOWNLOADRETRIES=5
COMMONLIBRARY_RETRYWAITTIMEINSECONDS=30
COMMONLIBRARY_VERBOSE=false
unknownproperties=''

while (($# > 0)); do
    lowerI="$(echo $1 | awk '{print tolower($0)}')"
    case $lowerI in
      --baseuri)
        baseuri=$2
        shift 1
        ;;
      --clean)
        clean=true
        ;;
      --force)
        force=$1
        ;;
      --downloadretries)
        COMMONLIBRARY_DOWNLOADRETRIES=$2
        shift 1
        ;;
      --retrywaittimeinseconds)
        COMMONLIBRARY_RETRYWAITTIMEINSECONDS=$2
        shift 1
        ;;
      --toolsversionsfile)
        toolsversionsfile=$2
        shift 1
        ;;
      --verbose)
        COMMONLIBRARY_VERBOSE=true
        export COMMONLIBRARY_VERBOSE
        ;;
      *)
        unknownproperties="$unknownproperties $1"
        ;;
    esac
    shift 1
done

if [[ $unknownproperties != '' ]]; then
    echo-error "Argument(s) $unknownproperties are not recognized"
    exit 1
fi

export COMMONLIBRARY_DOWNLOADRETRIES
export COMMONLIBRARY_RETRYWAITTIMEINSECONDS

# Import common library
source "$scriptroot/native/commonlibrary.sh"

function Main {
    reporoot="$scriptroot/../.."
    engcommonbasedir="$scriptroot/native"
    artifactsnativebasedir="$reporoot/artifacts/native"
    artifactsinstallbin="$artifactsnativebasedir"

    if [[ "$clean" == true || "$force" == "--force" ]]; then
        echo "Cleaning $artifactsnativebasedir"
        if [[ -z $artifactsnativebasedir ]]; then
            rm -dfr $artifactsnativebasedir
        fi

        if [[ "$clean" == true ]]; then
            exit 0
        fi
    fi

    echo "Processing $toolsversionsfile"
    if [[ ! -f $toolsversionsfile ]]; then
        echo_error "No native tool dependencies are defined in '$toolsversionsfile'"
        exit 0
    fi
    declare -A tools
    # TODO: this method requires that the txt file end in a newline
    while IFS='=' read -r toolname toolversion; do
        tools[$toolname]=$toolversion
        echo_verbose "$toolname $toolversion"
    done < $toolsversionsfile

    echo "Executing installers"
    for tool in "${!tools[@]}"; do 
        toolname=$tool
        toolversion=${tools[$tool]}
        toolfilename="$engcommonbasedir/install-$toolname.sh"

        if [[ ! -f $toolfilename ]]; then
            echo_error "$toolfilename does not exist.  Is $toolname a supported tool?"
            exit 1
        fi

        localinstallercommand="$toolfilename"
        localinstallercommand+=" --installpath $artifactsinstallbin"
        localinstallercommand+=" --baseuri $baseuri"
        localinstallercommand+=" --commonlibrarydirectory $engcommonbasedir"
        localinstallercommand+=" --version $toolversion"
        localinstallercommand+=" $force"

        echo "Installing $toolname $toolversion"
        echo_verbose "Executing '$localinstallercommand'"
        eval $localinstallercommand
    done

    if [[ -d $artifactsinstallbin ]]; then
        echo "Native tools are avilable from $artifactsinstallbin"
    else
        echo_error "Native tools install directory does not exist, installation failed"
        exit 1
    fi
    exit 0
}

Main