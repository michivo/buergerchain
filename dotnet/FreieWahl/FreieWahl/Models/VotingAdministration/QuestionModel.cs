using System.Collections.Generic;
using System.Linq;
using FreieWahl.Voting.Models;

namespace FreieWahl.Models.VotingAdministration
{
    public class QuestionModel
    {
        public QuestionModel(Question q)
        {
            Text = q.QuestionText;
            Description = _GetDescription(q.Details);
            Index = q.QuestionIndex;
            AnswerOptions = q.AnswerOptions.Select(x => new AnswerModel(x)).ToList();
            Type = q.QuestionType;
            MinNumAnswers = q.MinNumAnswers;
            MaxNumAnswers = q.MaxNumAnswers;
        }

        private string _GetDescription(List<QuestionDetail> details)
        {
            var description = details?.FirstOrDefault(x => x.DetailType == QuestionDetailType.AdditionalInfo);
            return description != null ? description.DetailValue : string.Empty;
        }

        public QuestionModel()
        {
        }

        public string Text { get; set; }
        public string Description { get; set; }
        public QuestionType Type { get; set; }
        public int MinNumAnswers { get; set; }
        public int MaxNumAnswers { get; set; }
        public List<AnswerModel> AnswerOptions { get; set; }
        public int Index { get; set; }
    }
}
