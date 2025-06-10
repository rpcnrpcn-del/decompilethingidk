using System.IO;
using System.Linq;
using System.Security.Cryptography;

public static class StringCipher
{
	private const int Keysize = 256;

	private const int DerivationIterations = 1000;

	public static byte[] Encrypt(byte[] plainTextBytes, string passPhrase)
	{
		byte[] array = Generate256BitsOfRandomEntropy();
		byte[] array2 = Generate256BitsOfRandomEntropy();
		Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(passPhrase, array, 1000);
		byte[] bytes = rfc2898DeriveBytes.GetBytes(32);
		using (RijndaelManaged rijndaelManaged = new RijndaelManaged())
		{
			rijndaelManaged.BlockSize = 256;
			rijndaelManaged.Mode = CipherMode.CBC;
			rijndaelManaged.Padding = PaddingMode.PKCS7;
			using (ICryptoTransform transform = rijndaelManaged.CreateEncryptor(bytes, array2))
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
					{
						cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
						cryptoStream.FlushFinalBlock();
						byte[] first = array;
						first = first.Concat(array2).ToArray();
						first = first.Concat(memoryStream.ToArray()).ToArray();
						memoryStream.Close();
						cryptoStream.Close();
						return first;
					}
				}
			}
		}
	}

	public static byte[] Decrypt(byte[] cipherTextBytesWithSaltAndIv, string passPhrase)
	{
		byte[] salt = cipherTextBytesWithSaltAndIv.Take(32).ToArray();
		byte[] rgbIV = cipherTextBytesWithSaltAndIv.Skip(32).Take(32).ToArray();
		byte[] array = cipherTextBytesWithSaltAndIv.Skip(64).Take(cipherTextBytesWithSaltAndIv.Length - 64).ToArray();
		Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(passPhrase, salt, 1000);
		byte[] bytes = rfc2898DeriveBytes.GetBytes(32);
		using (RijndaelManaged rijndaelManaged = new RijndaelManaged())
		{
			rijndaelManaged.BlockSize = 256;
			rijndaelManaged.Mode = CipherMode.CBC;
			rijndaelManaged.Padding = PaddingMode.PKCS7;
			using (ICryptoTransform transform = rijndaelManaged.CreateDecryptor(bytes, rgbIV))
			{
				using (MemoryStream memoryStream = new MemoryStream(array))
				{
					using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read))
					{
						byte[] array2 = new byte[array.Length];
						cryptoStream.Read(array2, 0, array2.Length);
						memoryStream.Close();
						cryptoStream.Close();
						return array2;
					}
				}
			}
		}
	}

	private static byte[] Generate256BitsOfRandomEntropy()
	{
		byte[] array = new byte[32];
		RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider();
		rNGCryptoServiceProvider.GetBytes(array);
		return array;
	}
}
