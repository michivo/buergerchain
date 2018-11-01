﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreieWahl.Voting.Models;
using Google.Cloud.Firestore;

namespace FreieWahl.Voting.Storage
{
    public class VotingFireStore : IVotingStore
    {
        private readonly FirestoreDb _db;
        private readonly string _collection = "Votings";

        public VotingFireStore(string projectId, string collectionPrefix = "")
        {
            _collection = collectionPrefix + _collection;
            _db = FirestoreDb.Create(projectId);
        }

        public async Task Insert(StandardVoting voting)
        {
            var dict = _ToDictionary(voting);
            var refId = await _db.Collection(_collection).AddAsync(dict).ConfigureAwait(false);
            voting.Id = refId.Id;
        }
        
        public Task Update(StandardVoting voting)
        {
            var docRef = _db.Collection(_collection).Document(voting.Id);
            return docRef.SetAsync(voting);
        }

        public async Task<StandardVoting> GetById(string id)
        {
            var docRef = _db.Collection(_collection).Document(id);
            var snapshot = await docRef.GetSnapshotAsync();
            if (!snapshot.Exists)
                return null;

            return _VotingFromDictionary(snapshot.ToDictionary(), snapshot.Id);
        }

        public async Task<IEnumerable<StandardVoting>> GetForUserId(string userId)
        {
            Query query = _db.Collection(_collection).WhereEqualTo("Creator", userId);
            return await _GetVotings(query).ConfigureAwait(false);
        }

        public async Task<IEnumerable<StandardVoting>> GetAll()
        {
            Query query = _db.Collection(_collection);
            return await _GetVotings(query).ConfigureAwait(false);
        }

        private async Task<IEnumerable<StandardVoting>> _GetVotings(Query query)
        {
            var snapshot = await query.GetSnapshotAsync().ConfigureAwait(false);
            var result = new List<StandardVoting>();
            foreach (var snapshotDocument in snapshot.Documents)
            {
                if (!snapshotDocument.Exists)
                    continue;

                result.Add(_VotingFromDictionary(snapshotDocument.ToDictionary(), snapshotDocument.Id));
            }

            return result;
        }

        private StandardVoting _VotingFromDictionary(Dictionary<string, object> document, string id)
        {
            var result = new StandardVoting()
            {
                Creator = (string) document["Creator"],
                ImageData = (string) document["ImageData"],
                Id = id,
                CurrentQuestionIndex = Convert.ToInt32(document["CurrentQuestionIndex"]),
                StartDate = ((Timestamp) document["StartDate"]).ToDateTime(),
                EndDate = ((Timestamp) document["EndDate"]).ToDateTime(),
                DateCreated = ((Timestamp) document["DateCreated"]).ToDateTime(),
                Description = (string) document["Description"],
                Title = (string) document["Title"],
                State = (VotingState)Convert.ToInt32(document["State"]),
                Visibility = (VotingVisibility)Convert.ToInt32(document["Visibility"]),
                Questions = ((IEnumerable<object>) document["Questions"]).Select(_QuestionFromObject).ToList()
            };
            return result;
        }

        private Question _QuestionFromObject(object document)
        {
            var dict = (Dictionary<string, object>) document;
            var result = new Question
            {
                QuestionIndex = Convert.ToInt32(dict["QuestionIndex"]),
                QuestionType = (QuestionType) Convert.ToInt32(dict["QuestionType"]),
                MinNumAnswers = Convert.ToInt32(dict["MinNumAnswers"]),
                MaxNumAnswers = Convert.ToInt32(dict["MaxNumAnswers"]),
                QuestionText = (string) dict["QuestionText"],
                Status = (QuestionStatus) Convert.ToInt32(dict["Status"]),
                AnswerOptions = ((IEnumerable<object>)dict["AnswerOptions"]).Select(_AnswerFromObject).ToList(),
                Details = ((IEnumerable<object>)dict["Details"]).Select(_DetailFromObject).ToList()
            };

            return result;
        }

        private QuestionDetail _DetailFromObject(object document)
        {
            var dict = (Dictionary<string, object>)document;
            return new QuestionDetail
            {
                DetailValue = (string) dict["DetailValue"],
                DetailType = (QuestionDetailType) Convert.ToInt32(dict["DetailType"]),
            };
        }

        private AnswerDetail _AnswerDetailFromObject(object document)
        {
            var dict = (Dictionary<string, object>)document;
            return new AnswerDetail
            {
                DetailValue = (string)dict["DetailValue"],
                DetailType = (AnswerDetailType)Convert.ToInt32(dict["DetailType"]),
            };
        }

        private AnswerOption _AnswerFromObject(object document)
        {
            var dict = (Dictionary<string, object>)document;
            var result = new AnswerOption
            {
                Id = (string)dict["Id"],
                AnswerText = (string)dict["AnswerText"],
                Details = ((IEnumerable<object>)dict["Details"]).Select(_AnswerDetailFromObject).ToList()
            };

            return result;
        }

        public async Task ClearAll()
        {
            var collectionReference = _db.Collection(_collection);
            int batchSize = 100;

            QuerySnapshot snapshot = await collectionReference.Limit(batchSize).GetSnapshotAsync();
            IReadOnlyList<DocumentSnapshot> documents = snapshot.Documents;
            while (documents.Count > 0)
            {
                foreach (DocumentSnapshot document in documents)
                {
                    await document.Reference.DeleteAsync();
                }

                snapshot = await collectionReference.Limit(batchSize).GetSnapshotAsync();
                documents = snapshot.Documents;
            }
        }

        public Task AddQuestion(string votingId, Question question)
        {
            DocumentReference votingRef = _db.Collection(_collection).Document(votingId);

            return votingRef.UpdateAsync("Questions", FieldValue.ArrayUnion(_ToDictionary(question)));
        }

        public Task DeleteQuestion(string votingId, int questionIndex)
        {
            throw new NotImplementedException();
        }

        public Task ClearQuestions(string votingId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateQuestion(string votingId, Question question)
        {
            throw new NotImplementedException();
        }

        public Task UpdateState(string votingId, VotingState state)
        {
            DocumentReference votingRef = _db.Collection(_collection).Document(votingId);
            Dictionary<string, object> updates = new Dictionary<string, object>
            {
                { "State", state }
            };
            return votingRef.UpdateAsync(updates);
        }

        public Task Delete(string votingId)
        {
            var docRef = _db.Collection(_collection).Document(votingId);
            return docRef.DeleteAsync();
        }


        private Dictionary<string, object> _ToDictionary(StandardVoting voting)
        {
            var result = new Dictionary<string, object>
            {
                {"Title", voting.Title},
                {"Description", voting.Description ?? string.Empty },
                {"Creator", voting.Creator },
                {"Visibility", (int)voting.Visibility },
                {"State", (int)voting.State },
                {"CurrentQuestionIndex", voting.CurrentQuestionIndex },
                {"ImageData", voting.ImageData ?? string.Empty },
                {"DateCreated", Timestamp.FromDateTime(voting.DateCreated) },
                {"StartDate", Timestamp.FromDateTime(voting.StartDate) },
                {"EndDate", Timestamp.FromDateTime(voting.EndDate) }
            };
            var questions = new ArrayList(voting.Questions.Select(_ToDictionary).ToArray());
            result.Add("Questions", questions);

            return result;
        }

        private Dictionary<string, object> _ToDictionary(Question question)
        {
            var result = new Dictionary<string, object>
            {
                { "QuestionText", question.QuestionText },
                { "MaxNumAnswers", question.MaxNumAnswers },
                { "MinNumAnswers", question.MinNumAnswers },
                { "QuestionIndex", question.QuestionIndex },
                { "QuestionType", (int)question.QuestionType },
                { "Status", (int)question.Status }
            };
            var answers = new ArrayList(question.AnswerOptions.Select(_ToDictionary).ToArray());
            result.Add("AnswerOptions", answers);
            var details = new ArrayList(question.Details.Select(_ToDictionary).ToArray());
            result.Add("Details", details);

            return result;
        }

        private Dictionary<string, object> _ToDictionary(AnswerOption answer)
        {
            var result = new Dictionary<string, object>
            {
                { "Id", answer.Id },
                { "AnswerText", answer.AnswerText }
            };
            var details = new ArrayList(answer.Details.Select(_ToDictionary).ToArray());
            result.Add("Details", details);

            return result;
        }

        private Dictionary<string, object> _ToDictionary(QuestionDetail questionDetail)
        {
            var result = new Dictionary<string, object>
            {
                { "DetailValue", questionDetail.DetailValue },
                { "DetailType", (int)questionDetail.DetailType },
            };

            return result;
        }

        private Dictionary<string, object> _ToDictionary(AnswerDetail answerDetail)
        {
            var result = new Dictionary<string, object>
            {
                { "DetailValue", answerDetail.DetailValue },
                { "DetailType", (int)answerDetail.DetailType },
            };

            return result;
        }
    }
}

