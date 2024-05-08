using System;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System.IO;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Utilities.IO;
using System.Text;

#pragma warning disable 1591

namespace SQLAid.Community.Pgp
{

    public class PgpTasks
    {
        internal const int EncryptBufferSize = 1 << 16;


        #region PgpDecryptFile

        /// <summary>
        /// Decrypt the file using the private key.
        /// </summary>
        public static PgpDecryptResult DecryptFile(PgpDecryptInput input)
        {
            if (!File.Exists(input.InputFile))
                throw new FileNotFoundException($"Encrypted File [{input.InputFile}] not found.");

            if (!File.Exists(input.PrivateKeyFile))
                throw new FileNotFoundException($"Private Key File [{input.PrivateKeyFile}] not found.");

            if (string.IsNullOrEmpty(input.OutputFile))
                throw new ArgumentNullException(input.OutputFile);

            using (var inputStream = File.OpenRead(input.InputFile))
            {
                using (var keyIn = File.OpenRead(input.PrivateKeyFile))
                {
                    Decrypt(inputStream, keyIn, input.PassPhrase, input.OutputFile);
                }
            }
            var ret = new PgpDecryptResult
            {
                FilePath = input.OutputFile
            };

            return ret;
        }





        internal static bool Decrypt(Stream inputStream, Stream privateKeyStream, string passPhrase, string outputFile)
        {
            PgpPrivateKey sKey = null;
            PgpPublicKeyEncryptedData pbe = null;
            var pgpF = new PgpObjectFactory(PgpUtilities.GetDecoderStream(inputStream));
            // find secret key
            var pgpSec = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(privateKeyStream));
            var o = pgpF.NextPgpObject();

            PgpEncryptedDataList enc;
            // the first object might be a PGP marker packet.
            if (o is PgpEncryptedDataList list)
                enc = list;
            else
                enc = (PgpEncryptedDataList)pgpF.NextPgpObject();

            // decrypt
            foreach (PgpPublicKeyEncryptedData pked in enc.GetEncryptedDataObjects())
            {
                sKey = PgpServices.FindSecretKey(pgpSec, pked.KeyId, passPhrase.ToCharArray());

                if (sKey == null) continue;
                pbe = pked;
                break;
            }

            if (sKey == null)
                throw new ArgumentException("Secret key for message not found.");

            PgpObjectFactory plainFact;

            using (var clear = pbe.GetDataStream(sKey))
            {
                plainFact = new PgpObjectFactory(clear);
            }

            var message = plainFact.NextPgpObject();

            // Some messages start with a signature list, which we need to get over
            // to get to the actual content of the message. Signature verification
            // should be done by VerifyFileSignature task.
            if (message is PgpSignatureList)
                message = plainFact.NextPgpObject();

            switch (message)
            {
                case PgpCompressedData cData:
                {
                    PgpObjectFactory of;

                    using (var compDataIn = cData.GetDataStream())
                    {
                        of = new PgpObjectFactory(compDataIn);
                    }

                    message = of.NextPgpObject();
                    if (message is PgpOnePassSignatureList)
                    {
                        message = of.NextPgpObject();
                        var ld = (PgpLiteralData)message;
                        using (var output = File.Create(outputFile))
                        {
                            var unc = ld.GetInputStream();
                            Streams.PipeAll(unc, output);
                        }
                    }
                    else
                    {
                        var ld = (PgpLiteralData)message;
                        using (var output = File.Create(outputFile))
                        {
                            var unc = ld.GetInputStream();
                            Streams.PipeAll(unc, output);
                        }
                    }

                    break;
                }
                case PgpLiteralData ld:
                {
                    using (var fOut = File.Create(outputFile))
                    {
                        var unc = ld.GetInputStream();
                        Streams.PipeAll(unc, fOut);
                    }

                    break;
                }
                case PgpOnePassSignatureList _:
                    throw new PgpException("Encrypted message contains a signed message - not literal data.");
                default:
                    throw new PgpException("Message is not a simple encrypted file - type unknown.");
            }

            return true;
        }

        #endregion

        #region PgpEncryptFile

        /// <summary>
        /// Encrypts a file using public key.
        /// If needed, the file can also be signed with private key, in this case, the order is sign and encrypt.
        /// See https://github.com/CommunityHiQ/Frends.Community.PgpEncryptFile
        /// </summary>
        /// <param name="input">Task input</param>
        /// <returns>Returns: Object {string FilePath}</returns>
        public static PgpEncryptResult EncryptFile(PgpEncryptInput input)
        {
            // source file to encrypt
            var inputFile = new FileInfo(input.InputFile);

            if (!inputFile.Exists)
                throw new ArgumentException("File to encrypt does not exists", input.InputFile);

            // destination file
            using (Stream outputStream = File.OpenWrite(input.OutputFile))
            // ascii output?
            using (var armoredStream = input.UseArmor ? new ArmoredOutputStream(outputStream) : outputStream)
            using (var encryptedOut = PgpServices.GetEncryptionStream(armoredStream, input))
            using (var compressedOut = PgpServices.GetCompressionStream(encryptedOut, input))
            {
                // signature init - if necessary
                var signatureGenerator = input.SignWithPrivateKey ? PgpServices.InitPgpSignatureGenerator(compressedOut, input) : null;

                // writing to configured output
                var literalDataGenerator = new PgpLiteralDataGenerator();
                var file = new FileInfo(input.InputFile);
                using (var literalOut = literalDataGenerator.Open(compressedOut, PgpLiteralData.Binary, file.Name, file.Length, DateTime.Now))
                using (var inputStream = inputFile.OpenRead())
                {
                    var buf = new byte[EncryptBufferSize];
                    int len;

                    while ((len = inputStream.Read(buf, 0, buf.Length)) > 0)
                    {
                        literalOut.Write(buf, 0, len);
                        if (input.SignWithPrivateKey)
                        {
                            signatureGenerator.Update(buf, 0, len);
                        }
                    }

                    if (input.SignWithPrivateKey)
                    {
                        signatureGenerator.Generate().Encode(compressedOut);
                    }
                }
            }

            return new PgpEncryptResult
            {
                FilePath = input.OutputFile
            };


        }

        #endregion

        #region PgpSignFile

        /// <summary>
        /// Sign a file with PGP signature. See documentation at https://github.com/CommunityHiQ/Frends.Community.PgpSignature Returns: Object {string FilePath}
        /// </summary>
        public static PgpSignatureResult SignFile(PgpSignatureInput input)
        {
            HashAlgorithmTag digest = input.HashFunction.ConvertEnum<HashAlgorithmTag>();

            using (var privateKeyStream = File.OpenRead(input.PrivateKeyFile))
            {
                var pgpSecKey = PgpServices.SignatureReadSecretKey(privateKeyStream);
                var pgpPrivKey = pgpSecKey.ExtractPrivateKey(input.Password.ToCharArray());
                var signatureGenerator = new PgpSignatureGenerator(pgpSecKey.PublicKey.Algorithm, digest);
                var signatureSubpacketGenerator = new PgpSignatureSubpacketGenerator();

                signatureGenerator.InitSign(PgpSignature.BinaryDocument, pgpPrivKey);

                var enumerator = pgpSecKey.PublicKey.GetUserIds().GetEnumerator();
                if (enumerator.MoveNext())
                {
                    signatureSubpacketGenerator.SetSignerUserId(false, (string)enumerator.Current);
                    signatureGenerator.SetHashedSubpackets(signatureSubpacketGenerator.Generate());
                }

                using (var outputStream = File.Create(input.OutputFile))
                {
                    var armoredOutputStream = new ArmoredOutputStream(outputStream);

                    var bcbgOutputStream = new BcpgOutputStream(armoredOutputStream);
                    signatureGenerator.GenerateOnePassVersion(false).Encode(bcbgOutputStream);

                    var file = new FileInfo(input.InputFile);
                    var literalDataGenerator = new PgpLiteralDataGenerator();
                    var literalDataOut = literalDataGenerator.Open(bcbgOutputStream, PgpLiteralData.Binary, file.Name, file.Length, DateTime.Now);
                    using (var fileIn = file.OpenRead())
                    {
                        int ch;

                        while ((ch = fileIn.ReadByte()) >= 0)
                        {
                            literalDataOut.WriteByte((byte)ch);
                            signatureGenerator.Update((byte)ch);
                        }

                        fileIn.Close();
                        literalDataGenerator.Close();
                        signatureGenerator.Generate().Encode(bcbgOutputStream);
                        armoredOutputStream.Close();
                        outputStream.Close();

                        var ret = new PgpSignatureResult
                        {
                            FilePath = input.OutputFile
                        };
                        return ret;
                    }
                }
            }
        }

        #endregion

        #region PgpVerifyFileSignature
        /// <summary>
        /// Verifies a PGP signature. See documentation at https://github.com/CommunityHiQ/Frends.Community.PgpVerifySignature Returns: Object {string FilePath, Boolean Verified}
        /// </summary>
        public static PgpVerifySignatureResult VerifyFileSignature(PgpVerifySignatureInput input)
        {

            using (var inputStream = PgpUtilities.GetDecoderStream(File.OpenRead(input.InputFile)))
            using (var keyStream = PgpUtilities.GetDecoderStream(File.OpenRead(input.PublicKeyFile)))
            {
                var pgpFact = new PgpObjectFactory(inputStream);
                var signatureList = (PgpOnePassSignatureList)pgpFact.NextPgpObject();

                if (signatureList == null)
                {
                    throw new Exception("Can't find signature in file.");
                }

                var onePassSignature = signatureList[0];



                var p2 = (PgpLiteralData)pgpFact.NextPgpObject();
                var dataIn = p2.GetInputStream();
                var pgpRing = new PgpPublicKeyRingBundle(keyStream);
                var key = pgpRing.GetPublicKey(onePassSignature.KeyId);

                string outputPath;
                if (string.IsNullOrWhiteSpace(input.OutputFolder))
                {
                    outputPath = Path.Combine(Path.GetDirectoryName(input.InputFile) ?? throw new ArgumentNullException(input.InputFile), p2.FileName);
                }
                else
                {
                    outputPath = Path.Combine(input.OutputFolder, p2.FileName);
                }
                using (var outputStream = File.Create(outputPath))
                {
                    onePassSignature.InitVerify(key);

                    int ch;
                    while ((ch = dataIn.ReadByte()) >= 0)
                    {
                        onePassSignature.Update((byte)ch);
                        outputStream.WriteByte((byte)ch);
                    }
                    outputStream.Close();
                }

                bool verified;
                // Will throw Exception if file is altered
                try
                {
                    var p3 = (PgpSignatureList)pgpFact.NextPgpObject();
                    var firstSig = p3[0];
                    verified = onePassSignature.Verify(firstSig);
                }
                catch (Exception)
                {
                    var retError = new PgpVerifySignatureResult
                    {
                        FilePath = input.OutputFolder,
                        Verified = false
                    };

                    return retError;
                }

                var ret = new PgpVerifySignatureResult
                {
                    FilePath = outputPath,
                    Verified = verified
                };

                return ret;
            }
        }

        #endregion

        #region PgpClearTextSignFile
        /// <summary>
        /// Create a file with PGP clear text signature. See documentation at https://github.com/CommunityHiQ/Frends.Community.PgpClearTextSignature Returns: Object {string FilePath}
        /// </summary>
        public static PgpClearTextSignatureResult ClearTextSignFile(PgpClearTextSignatureInput input)
        {
            HashAlgorithmTag digest;
            switch (input.HashFunction)
            {
                case PgpClearTextSignatureHashFunctionType.Md5:
                    digest = HashAlgorithmTag.MD5;
                    break;
                case PgpClearTextSignatureHashFunctionType.RipeMd160:
                    digest = HashAlgorithmTag.RipeMD160;
                    break;
                case PgpClearTextSignatureHashFunctionType.Sha1:
                    digest = HashAlgorithmTag.Sha1;
                    break;
                case PgpClearTextSignatureHashFunctionType.Sha224:
                    digest = HashAlgorithmTag.Sha224;
                    break;
                case PgpClearTextSignatureHashFunctionType.Sha384:
                    digest = HashAlgorithmTag.Sha384;
                    break;
                case PgpClearTextSignatureHashFunctionType.Sha512:
                    digest = HashAlgorithmTag.Sha512;
                    break;
                case PgpClearTextSignatureHashFunctionType.Sha256:
                    digest = HashAlgorithmTag.Sha256;
                    break;
                default:
                    digest = HashAlgorithmTag.Sha256;
                    break;
            }

            var privateKeyStream = File.OpenRead(input.PrivateKeyFile);

            var pgpSecKey = PgpServices.ReadSecretKey(privateKeyStream);
            var pgpPrivKey = pgpSecKey.ExtractPrivateKey(input.Password.ToCharArray());
            var sGen = new PgpSignatureGenerator(pgpSecKey.PublicKey.Algorithm, digest);
            var spGen = new PgpSignatureSubpacketGenerator();

            sGen.InitSign(PgpSignature.CanonicalTextDocument, pgpPrivKey);

            var enumerator = pgpSecKey.PublicKey.GetUserIds().GetEnumerator();
            if (enumerator.MoveNext())
            {
                spGen.SetSignerUserId(false, (string)enumerator.Current);
                sGen.SetHashedSubpackets(spGen.Generate());
            }

            var fIn = File.OpenRead(input.InputFile);
            var outputStream = File.Create(input.OutputFile);

            var aOut = new ArmoredOutputStream(outputStream);

            aOut.BeginClearText(digest);

            //
            // note the last \n/\r/\r\n in the file is ignored
            //
            var lineOut = new MemoryStream();
            var lookAhead = PgpServices.ReadInputLine(lineOut, fIn);

            PgpServices.ProcessLine(aOut, sGen, lineOut.ToArray());

            while (lookAhead != -1)
            {
                lookAhead = PgpServices.ReadInputLine(lineOut, lookAhead, fIn);

                sGen.Update((byte)'\r');
                sGen.Update((byte)'\n');

                PgpServices.ProcessLine(aOut, sGen, lineOut.ToArray());
            }

            fIn.Close();

            aOut.EndClearText();

            var bOut = new BcpgOutputStream(aOut);

            sGen.Generate().Encode(bOut);

            aOut.Close();
            outputStream.Close();

            var ret = new PgpClearTextSignatureResult
            {
                FilePath = input.OutputFile
            };

            return ret;
        }

        #endregion

        #region PgpVerifyFileClearTextSignature 
        /// <summary>
        /// Verifies clear text PGP signature. See documentation at https://github.com/CommunityHiQ/Frends.Community.PgpVerifyClearTextSignature Returns: Object {string FilePath, Boolean Verified}
        /// </summary>
        public static PgpVerifyClearTextSignatureResult VerifyFileClearTextSignature(PgpVerifyClearTextSignatureInput input)
        {
            using (var inStr = File.OpenRead(input.InputFile))
            using (var outStr = File.Create(input.OutputFile))
            using (var keyStr = PgpUtilities.GetDecoderStream(File.OpenRead(input.PublicKeyFile)))
            {
                var aInputStr = new ArmoredInputStream(inStr);

                //
                // write out signed section using the local line separator.
                // note: trailing white space needs to be removed from the end of
                // each line RFC 4880 Section 7.1
                //
                var lineOut = new MemoryStream();
                var lookAhead = PgpServices.VerifyClearTextSignatureReadInputLine(lineOut, aInputStr);
                var lineSep = Encoding.ASCII.GetBytes(Environment.NewLine); 


                if (lookAhead != -1 && aInputStr.IsClearText())
                {
                    var line = lineOut.ToArray();
                    outStr.Write(line, 0, PgpServices.GetLengthWithoutSeparatorOrTrailingWhitespace(line));
                    outStr.Write(lineSep, 0, lineSep.Length);

                    while (lookAhead != -1 && aInputStr.IsClearText())
                    {
                        lookAhead = PgpServices.VerifyClearTextSignatureReadInputLine(lineOut, lookAhead, aInputStr);

                        line = lineOut.ToArray();
                        outStr.Write(line, 0, PgpServices.GetLengthWithoutSeparatorOrTrailingWhitespace(line));
                        outStr.Write(lineSep, 0, lineSep.Length);
                    }
                }
                else
                {
                    // a single line file
                    if (lookAhead != -1)
                    {
                        var line = lineOut.ToArray();
                        outStr.Write(line, 0, PgpServices.GetLengthWithoutSeparatorOrTrailingWhitespace(line));
                        outStr.Write(lineSep, 0, lineSep.Length);
                    }
                }
                outStr.Close();

                var pgpRings = new PgpPublicKeyRingBundle(keyStr);

                var pgpFact = new PgpObjectFactory(aInputStr);
                var p3 = (PgpSignatureList)pgpFact.NextPgpObject();
                var sig = p3[0];
                inStr.Close();


                sig.InitVerify(pgpRings.GetPublicKey(sig.KeyId));
                // read the input, making sure we ignore the last newline.
                bool verified;
                using (var sigIn = File.OpenRead(input.OutputFile))
                {
                    lookAhead = PgpServices.VerifyClearTextSignatureReadInputLine(lineOut, sigIn);
                    PgpServices.ProcessLine(sig, lineOut.ToArray());
                    while (lookAhead != -1)
                    {
                        lookAhead = PgpServices.VerifyClearTextSignatureReadInputLine(lineOut, lookAhead, sigIn);

                        sig.Update((byte)'\r');
                        sig.Update((byte)'\n');

                        PgpServices.ProcessLine(sig, lineOut.ToArray());
                    }

                    verified = sig.Verify();
                    sigIn.Close();
                }
                var ret = new PgpVerifyClearTextSignatureResult
                {
                    FilePath = input.OutputFile,
                    Verified = verified
                };

                return ret;
            }
        }

        #endregion

    }

}
