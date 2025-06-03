namespace WebApi.Service.Admin
{
    public interface IPaymentService
    {
       
            bool ThanhToan(string maHopDong, string maGiaoDich, string email, string phuongThuc);
    }

}
