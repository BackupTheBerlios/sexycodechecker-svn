using System;
using System.Collections.Generic;
using System.IO;

namespace Cluefultoys.Sexycodechecker {

    public class MsBuildReader {

        // this should be a violation
        private const string sccBuildNamespace = "http://limacat.googlepages.com/cluefultoys/scc/extension/msbuild/parameters.xsd";

        // this should be a violation too
        private const string readCompileTags = "//MsBuild:Compile[contains(@Include,'.cs') and (not(@Scc:Ignore) or @Scc:Ignore='false')]/@Include";

    }

}