# Design

The bootstrap web user interface is built using bootstrap 4.1 with some
customizations required for the blueish/grayish design. The customizations and
extensions to bootstrap can be found in freiewahl.scss located in /design/TO. If
you want to build the customized css, you can execute the build.bat-script.
The prerequisites for executing the build.bat-script are:
* You need to have SASS installed and an environment variable named
SASS_PATH pointing to the directory containing your sass.bat
* You need to have an environment variable named BOOTSTRAP_PATH
pointing to the root directory of a local copy of bootstrap. Buergerchain was
built using bootstrap 4.1 and not tested with earlier or later versions.
