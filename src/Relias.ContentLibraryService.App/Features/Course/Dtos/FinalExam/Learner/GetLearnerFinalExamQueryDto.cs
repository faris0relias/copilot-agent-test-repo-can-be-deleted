using MediatR;
using Relias.ContentLibraryService.Domain.Course.FinalExam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam;

namespace Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam.Learner
{
    public class GetLearnerFinalExamQueryDto
    {
        public required List<Guid> QuestionIds { get; set; }
        public bool IsCompleted { get; set; }
       
    }
}
