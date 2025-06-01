using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PCL.Neo.Helpers.Animation
{
    public class AnimationState(CancellationTokenSource token)
    {
        public bool IsFunished;

        private CancellationTokenSource _cancellationTokenSource { get; } = token;

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
