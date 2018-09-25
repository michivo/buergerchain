using System.Collections.Generic;
using System.Linq;
using FreieWahl.Voting.Models;

namespace FreieWahl.Models.VotingAdministration
{
    public class AnswerModel
    {
        public AnswerModel(AnswerOption x)
        {
            Id = x.Id;
            Answer = x.AnswerText;
            Description = _GetDescription(x.Details);
        }

        public AnswerModel()
        {
        }

        private string _GetDescription(List<AnswerDetail> details)
        {
            var description = details?.FirstOrDefault(x => x.DetailType == AnswerDetailType.AdditionalInfo);
            return description != null ? description.DetailValue : string.Empty;
        }

        public string Id { get; set; }
        public string Answer { get; set; }
        public string Description { get; set; }
    }
}
