using test0000001.Models;
using test0000001.Models.DTO.LifeInsurance;
using test0000001.Models.LifeInsurance;

namespace test0000001.Repository.ServiceClass.LifeInsurance
{
    public class PolicyHolderViewService
    {
        private readonly PolicyHolderService _policyHolder;
        private readonly LifeInsuranceService _lifeInsurance;
        private readonly LifeInsuredObjectService _insuredObject;
        private readonly AppraisalInfoService _appraisalInfo;
        private readonly LifeInsuredDossiersService _dossier;
        private readonly PaymentScheduleService _schedule;

        public PolicyHolderViewService(
            PolicyHolderService policyHolder,
            LifeInsuranceService lifeInsurance,
            LifeInsuredObjectService insuredObject,
            AppraisalInfoService appraisalInfo,
            LifeInsuredDossiersService dossier,
            PaymentScheduleService schedule)
        {
            _policyHolder = policyHolder;
            _lifeInsurance = lifeInsurance;
            _insuredObject = insuredObject;
            _appraisalInfo = appraisalInfo;
            _dossier = dossier;
            _schedule = schedule;
        }

        public async Task<PackageOverviewDto?> GetPackageDetail(int? id)
        {
            if (id == null) return null;

            var holder = await GetPolicyholder((int)id);
            if (holder == null) return null;

            var insuredObj = await GetInsuredObject((int)id);
            if (insuredObj == null) return null;

            var user = await GetUser(holder.UserId!);
            var policy = _lifeInsurance.GetById(holder.PolicyId);
            var infos = await GetAppraisalInfos(insuredObj.Id);
            var dossier = await GetDossiers(insuredObj.Id);

            var schedules = await GetPaymentSchedules((int)id);

            if (user == null || policy == null ||
                infos == null || dossier == null) return null;

            return new PackageOverviewDto
            {
                User = user,
                PolicyHolder = holder,
                InsuredObject = insuredObj,
                AppraisalDossier = dossier,
                AppraisalInfos = infos,
                Package = policy,
                Payments = schedules
            };
        }

        public async Task<Policyholder?> GetPolicyholder(int id)
        {
            return await Task.FromResult(
                _policyHolder.GetById(id, asNoTracking: true));
        }

        public async Task<LifeInsuredObject?> GetInsuredObject(int id)
        {
            return await Task.FromResult(
                _insuredObject.GetByPolicyHolderId(id));
        }

        public async Task<ApplicationUser?> GetUser(string id)
        {
            return await Task.FromResult(
                _lifeInsurance.GetUserById(id, asNoTracking: true));
        }

        public async Task<AppraisalInfosDto> GetAppraisalInfos(int id)
        {
            return new AppraisalInfosDto
            {
                AppraisalInfos = await Task.FromResult(
                    _appraisalInfo.GetByInsuredObjectId(id))
            };
        }

        public async Task<AppraisalDossier?> GetDossiers(int id)
        {
            return await Task.FromResult(
                _dossier.GetByInsuredObjectId(id));
        }

        public async Task<List<PaymentScheduleDto>> GetPaymentSchedules(int id)
        {
            return await Task.FromResult(_schedule.GetByPolicyHolderId(id));
        }
    }
}
