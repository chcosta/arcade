# Use in the the functions: eval $invocation
invocation='echo_verbose "Calling: ${FUNCNAME[0]}"'

# PrintExitStatusMessage [exit code] ([error message]) ([message])
# Description: Prints message based on exit code result
# Parameters:
#   exit code - exit code of command
#   error message (optional) - message to print if 'exit code' is not '0'
#   message (optional) - message to print if 'exit code' is '0'
# Result: returns 'exit code'
function PrintExitStatusMessage {
    exitstatus=$1
    errormessage=''
    message=''
    if [[ "$#" -gt 1 ]]; then
        errormessage=$2
    fi
    if [[ "$#" -gt 2 ]]; then
        message=$3
    fi
    
    if [[ "$exitstatus" -eq '0' ]]; then
        echo "$message"
    else 
        echo_error "$errormessage"
    fi
    return $exitstatus
}

# echo_error [message]
# Description: Write text to the error stream
# Parameters:
#   message - text to write to the error stream
# Result: Write 'message' to the error stream
function echo_error {
    (>&2 echo "$1")
}

# echo_verbose [message]
# Description: Write verbose text if COMMONLIBRARY_VERBOSE env var is 'true'
# Parameters:
#   message - text to write to the output stream
# Result: Write verbose 'message' text
function echo_verbose {
    if [[ ! -z $COMMONLIBRARY_VERBOSE ]]; then
        if [[ "$COMMONLIBRARY_VERBOSE" == "true" ]]; then
            echo "Verbose: $1"
        fi
    fi
}

# DownloadAndExtract [uri] [install directory] (--force)
# Description: Wrapper method around the GetFile and ExpandTar methods
# Parameters:
#   uri - Uri to archive to download or copy locally
#   install directory - directory to extract contents of uri archive to
#   --force (optional) - specifies whether to remove contents before download and extract
# Result: Sets exit code to '0' for success, '1' for failure
function DownloadAndExtract {
    eval $invocation
    uri=$1
    installdirectory=$2
    force=''
    if [[ $# -gt 2 ]]; then
      force=$3
    fi
    tempdir="."
    toolfilename=${uri##*/}
    temptoolpath=$( GetTempPath )
    GetFile "$uri" "$temptoolpath" $force
    if [[ $? -ne 0 ]]; then
        echo_error "Download failed"
        return 1
    fi
    ExpandTar "$temptoolpath" "$installdirectory" $force
    if [[ $? -ne 0 ]]; then
        echo_error "Extract failed"
        return 1
    fi
    return 0
}

# ExpandTar [tar path] [output directory] (--force)
# Description: Extract tar contents to an output directory
# Parameters:
#   tar path - path to local tar file
#   output directory - directory to output contents to
#   --force (optional) -specifies whether to remove contents before extract
# Result: Sets exit code to '0' for success, '1' for failure
function ExpandTar {
    eval $invocation
    tarpath=$1
    outputdirectory=$2
    force=''
    if [[ $# -gt 2 ]]; then
      force=$3
    fi
    
    if [[ -d $outputdirectory ]]; then
        if [[ "$force" == "--force" ]]; then
            echo_verbose "Removing output directory '$outputdirectory'"
            rm -dfr $outputdirectory
        else
            echo "Output directory '$outputdirectory' already exists, skipping extract"
            return 0
        fi
    fi

    mkdir -p $outputdirectory
    echo_verbose "Executing 'tar -xf $tarpath -C $outputdirectory'"
    tar -zxf $tarpath -C $outputdirectory
    PrintExitStatusMessage $? "Extract failed"
    return $?
}

# GetArch
# Description: Gets the arch of the current OS. Use like 'arch=$( GetArch )'
function GetArch {
    cpuname=$( uname -p )
    # Some Linux platforms report unknown for platform, but the arch for machine
    if [[ "$cpuname" == "unknown" ]]; then
        cpuname=$( uname -m )
    fi
    echo $cpuname
}

# GetFriendlyArch
# Description: Get friendly arch names which are commonly used. Use like 'arch=$( GetFriendlyArch )'
function GetFriendlyArch {
    cpuname="$( GetArch )"
    case $cpuname in
        i686)
            echo "x86"
            return
            ;;
        x86_64)
            echo "x64"
            return
            ;;
        armv71)
            echo "arm"
            return
            ;;
        aarch64)
            echo "arm64"
            return
            ;;
        amd64)
            echo "x64"
            return
            ;;
    esac
    echo "x64"
}

# GetFile [uri] [path] (--force)
# Description: Download or copy a Uri files.  If Uri is a URL, then file is
#              downloaded.  If Uri is a local file, then file is copied.
# Parameters:
#   uri - source url or filename
#   path - destination for uri
#   --force (optional) -- specifies whether to remove path before download or copy
# Result: Sets exit code to '0' for success, '1' for failure
function GetFile {
    eval $invocation
    uri=$1
    path=$2
    force=''
    retries=5
    retrywaittimeinseconds=30
    if [[ $# -gt 2 ]]; then
      force=$3
    fi
    
    if [[ ! -z $COMMONLIBRARY_DOWNLOADRETRIES ]]; then
        retries=$COMMONLIBRARY_DOWNLOADRETRIES
    fi
    if [[ ! -z $COMMONLIBRARY_RETRYWAITTIMEINSECONDS ]]; then
        retrywaittimeinseconds=$COMMONLIBRARY_RETRYWAITTIMEINSECONDS
    fi

    if [[ "$force" == "--force" ]]; then
        if [[ -f $path ]]; then
            rm $path
        fi
    fi
    if [[ -f $path ]]; then
        echo "File '$path' already exists, skipping download"
        return 0
    fi

    if [[ -f $uri ]]; then
        echo_verbose "'$uri' is a file path, copying file to '$path'"
        cp "$uri" "$path"
        PrintExitStatusMessage $? "Copy failed"
        return $?
    else
        echo_verbose "Downloading $uri"
        if command -v curl > /dev/null; then
            STATUS=$(curl -sSL --create-dirs -w '%{http_code}' --retry $retries --retry-delay $retrywaittimeinseconds $uri -o $path)
            echo_verbose "Curl download status: $STATUS"
            if [[ $STATUS -eq 200 ]]; then
                echo_verbose "Download successful"
                return 0
            else
                echo_error "Download failed"
                return 1
            fi
        else
            wget -q --tries=$retries -waitretry=$retrywaittimeinseconds $uri -O $path
        fi
        PrintExitStatusMessage $? "Download failed"
        return $?
    fi
}

# GetOsName
# Description: Gets the name of the current OS. Use like 'os=$( GetOsName )'
function GetOsName {
    echo "$( uname -s )"
}

# GetTempPath
# Description: Get the name of a directory for temporary files. Use like 'tempdir=$( GetTempPath )'
function GetTempPath {
    echo "$HOME/.netcoreeng/native/temp"
}

# NewScriptShim [shim path] [tool file path] (--force)
# Description: Creates a wrapper script (shim) that passes arguments forward to native tool assembly
# Parameters:
#   shimpath - path to shim file
#   toolfilepath - path to assembly that shim forwards to
#   --force (optional) - specifies whether to delete old shim before attempting to write the generated one
# Result: Sets exit code to '0' for success, '1' for failure
function NewScriptShim {
    eval $invocation
    shimpath=$1
    toolfilepath=$2
    force=''
    if [[ $# -gt 2 ]]; then
      force=$3
    fi
    echo_verbose "Generating '$shimpath' shim"
    if [[ -f $shimpath ]] && [[ ! "$force" == "--force" ]]; then
        echo_error "'$shimpath' already exists"
        return 0
    fi

    if [[ ! -f $toolfilepath ]]; then
        echo_error "Specified tool file path '$toolfilepath' does not exist"
        return 1
    fi

    rm $shimpath
    echo "#!/usr/bin/env bash" > $shimpath
    echo "\"$toolfilepath\" \$@" >> $shimpath
    echo_verbose "Setting +x on $shimpath"
    chmod +x $shimpath
    echo "Created shim $shimpath"
    return 0
}