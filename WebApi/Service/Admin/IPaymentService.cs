using WebApi.DTO;

namespace WebApi.Service.Admin
{
    public interface IPaymentService
    {

        bool ThanhToan(PaymentTr request);
    }

}
