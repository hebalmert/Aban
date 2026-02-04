namespace Aban.AppInfra.QRgenerate;

public interface IQRService
{
    string GenerateQrBase64(string url);
}