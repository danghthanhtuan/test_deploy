namespace WebApi.Service.Admin
{
    public interface IPaymentService
    {

        bool ThanhToan(string ID, string maGiaoDich, string phuongThuc, string tinhTrang);
    }

}
