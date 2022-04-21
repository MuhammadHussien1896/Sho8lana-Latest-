namespace Sho8lana.Models.ViewModels
{
    public class ContractViewModel
    {
        public ContractViewModel()
        {
            ActiveContracts = new List<Contract>();
            DoneContracts = new List<Contract>();
        }
        public ICollection<Contract> ActiveContracts { get; set; }
        public ICollection<Contract> DoneContracts { get; set; }
    }
}
