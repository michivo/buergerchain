﻿using System;
using System.Text;
using System.Collections.Generic;
using FreieWahl.Security.Signing.Buergerkarte;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.FreieWahl.Security.Signing.Buergerkarte
{
    /// <summary>
    /// Summary description for Test_BuergerkarteSigning
    /// </summary>
    [TestClass]
    public class Test_BuergerkarteSigning
    {
        [TestMethod]
        public void TestMethod1()
        {
            SignatureHandler s = new SignatureHandler();
            string signedData =
                "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";
            string data = "Ich bin ein einfacher Text.";
            s.VerifySignature(data, signedData); 
        }
    }
}
