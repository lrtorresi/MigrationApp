using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentValidator.Domain.Entities.Workplace
{
    public class Vote
    {
        public string? Message { get; set; }
        public Poll? Poll { get; set; }
        public string? Id { get; set; }
    }

    public class Poll
    {
        public Option? Options { get; set; }
        public string? Id { get; set; }
    }

    public class Option
    {
        public List<DataVote>? Data { get; set; }
        public Paging? Paging { get; set; }
    }

    public class DataVote
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public int Vote_Count { get; set; }
        public VoteDetail? Votes { get; set; }
    }


    public class VoteDetail
    {
        public List<VoteData>? Data { get; set; }
        public Paging? Paging { get; set; }
    }

    public class VoteData
    {
        public string? Id { get; set; }
        public string? Name { get; set; }

    }
}

