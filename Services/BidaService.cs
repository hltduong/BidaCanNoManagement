using BidaCanNoManagement.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection;

namespace BidaCanNoManagement.Services
{
    public class BidaService
    {
        private List<Result> data;
        private readonly string dataFile = Path.Combine(Utils.GetCurrentDirectory(), "data.json");
        private readonly string historyFile = Path.Combine(Utils.GetCurrentDirectory(), "history.json");

        public BidaService() 
        {
            Load();
        }

        public List<Result> Load()
        {
            data = JsonConvert.DeserializeObject<List<Result>>(File.ReadAllText(dataFile));
            return data;
        }

        public void Save() 
        {
            File.WriteAllText(dataFile, JsonConvert.SerializeObject(data));
        }

        public void Update(Result item)
        {
            UpdateHistory(item);

            var queueOfChanges = new Queue<Result>();
            queueOfChanges.Enqueue(item);

            while (queueOfChanges.Count > 0)
            {
                var change = queueOfChanges.Dequeue();

                var subList1 = UpdateByLoserHistory(change);
                var subList2 = UpdateByWinnerHistory(change);

                if (change.Score > 0)
                {
                    data.Add(change);
                }

                foreach (var subItem in subList1)
                {
                    queueOfChanges.Enqueue(subItem);
                }

                foreach (var subItem in subList2)
                {
                    queueOfChanges.Enqueue(subItem);
                }
            }

            ReconcileScores();
            ClearEmptyScores();
        }

        private void UpdateHistory(Result item)
        {
            var historyData = JsonConvert.DeserializeObject<List<Result>>(File.ReadAllText(historyFile));
            historyData.Add(item);
            File.WriteAllText(historyFile, JsonConvert.SerializeObject(historyData));
        }

        private void ClearEmptyScores()
        {
            var emptyScores = data.Where(i => i.Score == 0).ToList();
            foreach (var emptyItem in emptyScores)
            {
                data.Remove(emptyItem);
            }
        }

        private void ReconcileScores()
        {
            var groupedItems = data.GroupBy(i => new { WinnerName = i.Winner.Name, LoserName = i.Loser.Name }).ToList();
            data.Clear();

            foreach (var groupedItem in groupedItems)
            {
                var totalScore = groupedItem.Sum(i => i.Score);

                data.Add(new Result
                {
                    Winner = new Player { Name = groupedItem.Key.WinnerName },
                    Loser = new Player { Name = groupedItem.Key.LoserName },
                    Score = Convert.ToUInt32(totalScore)
                });
            }
        }

        private List<Result> UpdateByWinnerHistory(Result item)
        {
            var resultsThatWinnerLost = data.Where(i => i.Loser.Name == item.Winner.Name).ToList();
            var index = 0;
            var newChanges = new List<Result>();

            while (index < resultsThatWinnerLost.Count && item.Score > 0)
            {
                // A > B = 3, B > C = 2
                // => A > B = 1, A > C = 2
                if (item.Score <= resultsThatWinnerLost[index].Score)
                {
                    newChanges.Add(new Result
                    {
                        Winner = resultsThatWinnerLost[index].Winner,
                        Loser = item.Loser,
                        Score = item.Score
                    });

                    resultsThatWinnerLost[index].Score -= item.Score;
                    item.Score = 0;
                }
                // A > B = 2, B > C = 3
                // => A > C = 2, B > C = 1
                else
                {
                    newChanges.Add(new Result
                    {
                        Winner = resultsThatWinnerLost[index].Winner,
                        Loser = item.Loser,
                        Score = resultsThatWinnerLost[index].Score
                    });

                    data.Remove(resultsThatWinnerLost[index]);
                    item.Score -= resultsThatWinnerLost[index].Score;
                }

                index++;
            }

            return newChanges;
        }

        private List<Result> UpdateByLoserHistory(Result item)
        {
            var resultsThatLoserWon = data.Where(i => i.Winner.Name == item.Loser.Name).ToList();
            var index = 0;
            var newChanges = new List<Result>();

            while (index < resultsThatLoserWon.Count && item.Score > 0)
            {
                // A > B = 3, B > C = 2
                // => A > B = 1, A > C = 2
                if (item.Score >= resultsThatLoserWon[index].Score)
                {
                    newChanges.Add(new Result
                    {
                        Winner = item.Winner,
                        Loser = resultsThatLoserWon[index].Loser,
                        Score = resultsThatLoserWon[index].Score
                    });

                    data.Remove(resultsThatLoserWon[index]);
                    item.Score -= resultsThatLoserWon[index].Score;
                }
                // A > B = 2, B > C = 3
                // => A > C = 2, B > C = 1
                else
                {
                    newChanges.Add(new Result
                    {
                        Winner = item.Winner,
                        Loser = resultsThatLoserWon[index].Loser,
                        Score = item.Score
                    });

                    resultsThatLoserWon[index].Score -= item.Score;
                    item.Score = 0;
                }

                index++;
            }

            return newChanges;
        }
    }
}
