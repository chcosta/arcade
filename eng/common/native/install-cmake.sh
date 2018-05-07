## Resolve script path
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

# Import common library header to parse args
source "$scriptroot/commonlibrary-installheader.sh"

## Define variables
toolname="cmake"
toolarch=$( GetArch )
toolnamemoniker="$toolname-$toolversion-Linux-$toolarch"
uri="$baseuri/$toolnamemoniker.tar.gz"

# We can't make the directory $installpath/$toolname/... because we generate
# a shim of the same name in installpath and bash will get confused if we tell
# it to remove a directory but it finds a file
toolinstalldirectory="$installpath/$toolname.bin/$toolversion"

## Download and install tool
DownloadAndExtract "$uri" "$toolinstalldirectory" "$force"
if [[ $? -eq 1 ]]; then
    echo_error "Installation failed"
    exit 1
fi

## Generate a shim
NewScriptShim "$installpath/cmake" "$toolinstalldirectory/$toolnamemoniker/bin/cmake" "--force"
if [[ $? -eq 1 ]]; then
    echo_error "Generate shim failed"
    exit 1
fi
exit 0
