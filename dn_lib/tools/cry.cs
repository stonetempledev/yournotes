using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Xml;

namespace dn_lib.tools {
  public class cry {

    #region sha

    public static string encode_tobase64 (string password) {
      return Convert.ToBase64String(HashAlgorithm.Create("SHA1").ComputeHash(Encoding.Unicode.GetBytes(password)));
    }

    #endregion

    #region random

    private static Random _rnd = new Random();
    public static string rnd_str (int length) {
      const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
      return new string(Enumerable.Repeat(chars, length).Select(s => s[_rnd.Next(s.Length)]).ToArray());
    }

    #endregion

    #region crypt

    private const int _keysz = 256;
    private const int _itrs = 1000;
    public static string encrypt (string plain_text, string pass_phrase) {
      var salt_bytes = gen_256_bits();
      var iv_bytes = gen_256_bits();
      var pl_bytes = Encoding.UTF8.GetBytes(plain_text);
      using (var pwd = new Rfc2898DeriveBytes(pass_phrase, salt_bytes, _itrs)) {
        var k_bytes = pwd.GetBytes(_keysz / 8);
        using (var s_key = new RijndaelManaged()) {
          s_key.BlockSize = 256;
          s_key.Mode = CipherMode.CBC;
          s_key.Padding = PaddingMode.PKCS7;
          using (var encry = s_key.CreateEncryptor(k_bytes, iv_bytes)) {
            using (var m_str = new MemoryStream()) {
              using (var cr_str = new CryptoStream(m_str, encry, CryptoStreamMode.Write)) {
                cr_str.Write(pl_bytes, 0, pl_bytes.Length);
                cr_str.FlushFinalBlock();
                var cipherTextBytes = salt_bytes;
                cipherTextBytes = cipherTextBytes.Concat(iv_bytes).ToArray();
                cipherTextBytes = cipherTextBytes.Concat(m_str.ToArray()).ToArray();
                m_str.Close(); cr_str.Close();
                return Convert.ToBase64String(cipherTextBytes);
              }
            }
          }
        }
      }
    }

    public static string decrypt (string c_text, string pass_phrase) {
      var ci_bytes = Convert.FromBase64String(c_text);
      var sal_bytes = ci_bytes.Take(_keysz / 8).ToArray();
      var iv_str = ci_bytes.Skip(_keysz / 8).Take(_keysz / 8).ToArray();
      var c_bytes = ci_bytes.Skip((_keysz / 8) * 2).Take(ci_bytes.Length - ((_keysz / 8) * 2)).ToArray();

      using (var pwd = new Rfc2898DeriveBytes(pass_phrase, sal_bytes, _itrs)) {
        var k_bytes = pwd.GetBytes(_keysz / 8);
        using (var s_key = new RijndaelManaged()) {
          s_key.BlockSize = 256;
          s_key.Mode = CipherMode.CBC;
          s_key.Padding = PaddingMode.PKCS7;
          using (var dec = s_key.CreateDecryptor(k_bytes, iv_str)) {
            using (var m_str = new MemoryStream(c_bytes)) {
              using (var c_str = new CryptoStream(m_str, dec, CryptoStreamMode.Read)) {
                var p_bytes = new byte[c_bytes.Length];
                var d_byte = c_str.Read(p_bytes, 0, p_bytes.Length);
                m_str.Close(); c_str.Close();
                return Encoding.UTF8.GetString(p_bytes, 0, d_byte);
              }
            }
          }
        }
      }
    }

    private static byte[] gen_256_bits () {
      var rnd_bytes = new byte[32];
      using (var rngCsp = new RNGCryptoServiceProvider()) { rngCsp.GetBytes(rnd_bytes); }
      return rnd_bytes;
    }

    #endregion

    #region crypt files

    public static bool file_encrypt (string path_file, string path_out, string pwd) {
      FileStream fs_crypt = new FileStream(path_out, FileMode.Create);
      byte[] salt = rnd_salt(), p_bytes = System.Text.Encoding.UTF8.GetBytes(pwd);
      RijndaelManaged aes = new RijndaelManaged() { KeySize = 256, BlockSize = 128, Padding = PaddingMode.PKCS7 };
      Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(p_bytes, salt, 100);
      aes.Key = key.GetBytes(aes.KeySize / 8);
      aes.IV = key.GetBytes(aes.BlockSize / 8);

      aes.Mode = CipherMode.CFB;

      fs_crypt.Write(salt, 0, salt.Length);

      CryptoStream cs = new CryptoStream(fs_crypt, aes.CreateEncryptor(), CryptoStreamMode.Write);

      FileStream fs_in = new FileStream(path_file, FileMode.Open);

      byte[] buffer = new byte[1048576];
      int read;

      try {
        while ((read = fs_in.Read(buffer, 0, buffer.Length)) > 0)
          cs.Write(buffer, 0, read);

        fs_in.Close();
      } catch (Exception ex) {
        log.log_err(ex);
        return false;
      } finally { cs.Close(); fs_crypt.Close(); }
      return true;
    }

    public static XmlDocument xml_decrypt (string path_file, string pwd) {
      byte[] p_bytes = System.Text.Encoding.UTF8.GetBytes(pwd), salt = new byte[32];

      XmlDocument res = new XmlDocument();

      FileStream fs_crypt = new FileStream(path_file, FileMode.Open);
      fs_crypt.Read(salt, 0, salt.Length);

      RijndaelManaged aes = new RijndaelManaged() { KeySize = 256, BlockSize = 128 };
      Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(p_bytes, salt, 100);
      aes.Key = key.GetBytes(aes.KeySize / 8);
      aes.IV = key.GetBytes(aes.BlockSize / 8);
      aes.Padding = PaddingMode.PKCS7;
      aes.Mode = CipherMode.CFB;

      CryptoStream cs = new CryptoStream(fs_crypt, aes.CreateDecryptor(), CryptoStreamMode.Read);
      //FileStream fsOut = new FileStream(path_out, FileMode.Create);
      StringBuilder str = new StringBuilder();
      int read; byte[] buffer = new byte[1048576];
      try {
        //while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
        //  fsOut.Write(buffer, 0, read);        
        while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
          str.Append(Encoding.Default.GetString(buffer, 0, read));
        res.LoadXml(str.ToString().Substring(3));
      } catch (CryptographicException ex_cry) {
        log.log_err("CryptographicException error: " + ex_cry.Message);
        return null;
      } catch (Exception ex) {
        log.log_err(ex);
        return null;
      }

      try {
        cs.Close();
      } catch (Exception ex) {
        log.log_err("Error by closing CryptoStream: " + ex.Message);
        return null;
      } finally { 
        //fsOut.Close(); 
        fs_crypt.Close();
      }

      return res;
    }

    public static byte[] rnd_salt () {
      byte[] data = new byte[32];
      using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider()) { for (int i = 0; i < 10; i++) rng.GetBytes(data); }
      return data;
    }

    #endregion
  }
}
