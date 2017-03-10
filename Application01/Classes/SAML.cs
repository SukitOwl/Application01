using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Xml;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography;
using Application01.helpers;
using Application01.Classes;

namespace Application01.Classes
{
    public class SamL
    {
    }

    public class Certificate
    {
        public X509Certificate2 cert;

        public void LoadCertificate(string name)
        {
            cert = new X509Certificate2();
            string path = System.AppDomain.CurrentDomain.BaseDirectory + "Store\\SSO\\" + name;
            cert.Import(path);
        }

        private byte[] StringToByteArray(string st)
        {
            byte[] bytes = new byte[st.Length];
            for (int i = 0; i < st.Length; i++)
            {
                bytes[i] = (byte)st[i];
            }
            return bytes;
        }
    }

    public class Response
    {
        private XmlDocument xmlDoc;
        private Certificate certificate;

        public Response()
        {
            certificate = new Certificate();
            certificate.LoadCertificate(helpers.Configuration.sso_certificate);
        }

        public void LoadXml(string xml)
        {
            xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.XmlResolver = null;
            xmlDoc.LoadXml(xml);
        }

        public void LoadXmlFromBase64(string response)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            LoadXml(enc.GetString(Convert.FromBase64String(response)));
        }

        public bool IsValid()
        {
            XmlNamespaceManager manager = new XmlNamespaceManager(xmlDoc.NameTable);
            manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            XmlNodeList nodeList = xmlDoc.SelectNodes("//ds:Signature", manager);
            SignedXml signedXml = new SignedXml(xmlDoc);
            signedXml.LoadXml((XmlElement)nodeList[0]);
            return signedXml.CheckSignature(certificate.cert, true);
        }

        public string GetNameID()
        {
            XmlNamespaceManager manager = new XmlNamespaceManager(xmlDoc.NameTable);
            manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            manager.AddNamespace("saml2", "urn:oasis:names:tc:SAML:2.0:assertion");
            manager.AddNamespace("saml2p", "urn:oasis:names:tc:SAML:2.0:protocol");
            XmlNode node = xmlDoc.SelectSingleNode("/saml2p:Response/saml2:Assertion/saml2:Subject/saml2:NameID", manager);
            return node.InnerText;
        }

        public string GetSessionIndex()
        {
            XmlNamespaceManager manager = new XmlNamespaceManager(xmlDoc.NameTable);
            manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            manager.AddNamespace("saml2", "urn:oasis:names:tc:SAML:2.0:assertion");
            manager.AddNamespace("saml2p", "urn:oasis:names:tc:SAML:2.0:protocol");
            XmlNode node = xmlDoc.SelectSingleNode("/saml2p:Response/saml2:Assertion/saml2:AuthnStatement", manager);
            return node.Attributes["SessionIndex"].Value;
        }

        public string GetLogoutStatus()
        {
            XmlNamespaceManager manager = new XmlNamespaceManager(xmlDoc.NameTable);
            manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            manager.AddNamespace("saml2", "urn:oasis:names:tc:SAML:2.0:assertion");
            manager.AddNamespace("saml2p", "urn:oasis:names:tc:SAML:2.0:protocol");
            XmlNode node = xmlDoc.SelectSingleNode("/saml2p:LogoutResponse/saml2p:Status/saml2p:StatusCode", manager);
            return node.Attributes["Value"].Value;
        }

        public string GetAttribute(string name)
        {
            XmlNamespaceManager manager = new XmlNamespaceManager(xmlDoc.NameTable);
            manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            manager.AddNamespace("saml2", "urn:oasis:names:tc:SAML:2.0:assertion");
            manager.AddNamespace("saml2p", "urn:oasis:names:tc:SAML:2.0:protocol");
            XmlNode node = xmlDoc.SelectSingleNode("/saml2p:Response/saml2:Assertion/saml2:AttributeStatement/saml2:Attribute[@Name=\"" + name + "\"]/saml2:AttributeValue", manager);
            //xmlDoc.Save("C:\\Users\\User\\Desktop\\RESPONSE.xml");
            return node.InnerText;
        }
    }

    public class AuthRequest
    {
        public string id;
        private string issue_instant;

        public enum AuthRequestFormat
        {
            Base64 = 1
        }

        public AuthRequest()
        {
            id = "_" + System.Guid.NewGuid().ToString();
            issue_instant = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        public string GetRequest(AuthRequestFormat format)
        {
            XmlDocument xmlDoc = new XmlDocument();
            using (StringWriter sw = new StringWriter())
            {
                XmlWriterSettings xws = new XmlWriterSettings();
                xws.OmitXmlDeclaration = true;

                using (XmlWriter xw = xmlDoc.CreateNavigator().AppendChild())
                {
                    xw.WriteStartElement("samlp", "AuthnRequest", "urn:oasis:names:tc:SAML:2.0:protocol");
                    xw.WriteAttributeString("ID", id);
                    xw.WriteAttributeString("Version", "2.0");
                    xw.WriteAttributeString("IssueInstant", issue_instant);
                    xw.WriteAttributeString("ProtocolBinding", "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST");
                    xw.WriteAttributeString("AssertionConsumerServiceURL", helpers.Configuration.sp_sso_target_url);
                    xw.WriteAttributeString("IsPassive", "false");
                    xw.WriteAttributeString("ForceAuthn", "false");
                    xw.WriteAttributeString("Destination", helpers.Configuration.idp_sso_destination_url);

                    xw.WriteStartElement("saml", "Issuer", "urn:oasis:names:tc:SAML:2.0:assertion");
                    xw.WriteString(helpers.Configuration.sso_issuer);
                    xw.WriteEndElement();

                    xw.WriteStartElement("samlp", "NameIDPolicy", "urn:oasis:names:tc:SAML:2.0:protocol");
                    xw.WriteAttributeString("Format", "urn:oasis:names:tc:SAML:2.0:nameid-format:unspecified");
                    xw.WriteAttributeString("AllowCreate", "true");
                    xw.WriteAttributeString("SPNameQualifier", "Issuer");
                    xw.WriteEndElement();

                    xw.WriteStartElement("samlp", "RequestedAuthnContext", "urn:oasis:names:tc:SAML:2.0:protocol");
                    xw.WriteAttributeString("Comparison", "exact");

                    xw.WriteStartElement("saml", "AuthnContextClassRef", "urn:oasis:names:tc:SAML:2.0:assertion");
                    xw.WriteString("urn:oasis:names:tc:SAML:2.0:ac:classes:PasswordProtectedTransport");
                    xw.WriteEndElement();

                    xw.WriteEndElement();

                    xw.WriteEndElement();
                }

                string cerName = helpers.Configuration.sso_cer;
                string cerPass = helpers.Configuration.sso_cer_pass;
                string cerPath = System.AppDomain.CurrentDomain.BaseDirectory + "Store\\SSO\\" + cerName;

                X509Certificate2 cert = new X509Certificate2(cerPath, cerPass, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
                var cerKey = cert.PrivateKey;
                RSACryptoServiceProvider rsaKey = (RSACryptoServiceProvider)cerKey;

                SignedXml signedXml = new SignedXml(xmlDoc);
                signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;
                signedXml.SigningKey = cerKey;
                Reference reference = new Reference();
                reference.Uri = "#" + id;
                XmlDsigExcC14NTransform env2 = new XmlDsigExcC14NTransform();
                XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
                reference.TransformChain.Add(env);
                reference.TransformChain.Add(env2);
                signedXml.AddReference(reference);
                signedXml.ComputeSignature();
                KeyInfo keyInfo = new KeyInfo();
                KeyInfoX509Data keyInfoData = new KeyInfoX509Data(cert);
                keyInfo.AddClause(keyInfoData);
                signedXml.KeyInfo = keyInfo;
                XmlElement xmlDigitalSignature = signedXml.GetXml();
                xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(xmlDigitalSignature, true));

                //xmlDoc.Save("C:\\Users\\User\\Desktop\\REQUEST.xml");

                if (format == AuthRequestFormat.Base64)
                {
                    XmlDocument doc2 = new XmlDocument();

                    byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(xmlDoc.OuterXml);
                    return System.Convert.ToBase64String(toEncodeAsBytes);
                }

                return null;
            }
        }
    }

    public class LogoutAuthRequest
    {
        public string id;
        private string issue_instant;
        private string not_On_Or_After;

        public enum AuthRequestFormat
        {
            Base64 = 1
        }

        public LogoutAuthRequest()
        {
            id = "_" + System.Guid.NewGuid().ToString();
            issue_instant = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
            not_On_Or_After = DateTime.Now.ToUniversalTime().AddSeconds(120).ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        public string GetLogoutRequest(AuthRequestFormat format)
        {
            XmlDocument xmlDoc = new XmlDocument();
            using (StringWriter sw = new StringWriter())
            {
                XmlWriterSettings xws = new XmlWriterSettings();
                xws.OmitXmlDeclaration = true;

                using (XmlWriter xw = xmlDoc.CreateNavigator().AppendChild())
                {
                    xw.WriteStartElement("samlp", "LogoutRequest", "urn:oasis:names:tc:SAML:2.0:protocol");
                    xw.WriteAttributeString("ID", id);
                    xw.WriteAttributeString("Version", "2.0");
                    xw.WriteAttributeString("IssueInstant", issue_instant);
                    xw.WriteAttributeString("Reason", "Single Logout");
                    xw.WriteAttributeString("NotOnOrAfter", not_On_Or_After);
                    xw.WriteAttributeString("Destination", helpers.Configuration.idp_sso_destination_url);

                    xw.WriteStartElement("saml", "Issuer", "urn:oasis:names:tc:SAML:2.0:assertion");
                    xw.WriteString(helpers.Configuration.sso_issuer);
                    xw.WriteEndElement();

                    xw.WriteStartElement("saml", "NameID", "urn:oasis:names:tc:SAML:2.0:assertion");
                    xw.WriteAttributeString("Format", "urn:oasis:names:tc:SAML:2.0:nameid-format:entity");
                    xw.WriteString(Configuration.SSONameId);
                    xw.WriteEndElement();

                    xw.WriteStartElement("samlp", "SessionIndex", "urn:oasis:names:tc:SAML:2.0:protocol");
                    xw.WriteString(Configuration.SSOSessionIndex);

                    xw.WriteEndElement();

                    xw.WriteEndElement();
                }

                string cerName = helpers.Configuration.sso_cer;
                string cerPass = helpers.Configuration.sso_cer_pass;
                string cerPath = System.AppDomain.CurrentDomain.BaseDirectory + "Store\\SSO\\" + cerName;

                X509Certificate2 cert = new X509Certificate2(cerPath, cerPass, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
                var cerKey = cert.PrivateKey;
                RSACryptoServiceProvider rsaKey = (RSACryptoServiceProvider)cerKey;

                SignedXml signedXml = new SignedXml(xmlDoc);
                signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;
                signedXml.SigningKey = cerKey;
                Reference reference = new Reference();
                reference.Uri = "#" + id;
                XmlDsigExcC14NTransform env2 = new XmlDsigExcC14NTransform();
                XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
                reference.TransformChain.Add(env);
                reference.TransformChain.Add(env2);
                signedXml.AddReference(reference);
                signedXml.ComputeSignature();
                KeyInfo keyInfo = new KeyInfo();
                KeyInfoX509Data keyInfoData = new KeyInfoX509Data(cert);
                keyInfo.AddClause(keyInfoData);
                signedXml.KeyInfo = keyInfo;
                XmlElement xmlDigitalSignature = signedXml.GetXml();
                xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(xmlDigitalSignature, true));
                if (format == AuthRequestFormat.Base64)
                {
                    byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(xmlDoc.OuterXml);
                    return System.Convert.ToBase64String(toEncodeAsBytes);
                }

                return null;
            }
        }
    }
}