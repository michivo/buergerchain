using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreieWahl.Models.VotingAdministration;
using Remotion.Linq;

namespace FreieWahl.Models.Voting
{
    public class QuestionData
    {
        public VotingQuestionModel[] Questions { get; set; }
    }
}
