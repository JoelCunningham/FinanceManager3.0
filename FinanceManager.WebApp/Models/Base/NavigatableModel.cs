namespace FinanceManager.WebApp.Models.Base
{
    public abstract class NavigatableModel<TStage> where TStage : struct, Enum
    {
        private int _currentStageIndex;
        protected abstract TStage[] Stages { get; }

        protected NavigatableModel()
        {
            if (Stages == null || Stages.Length == 0)
            {
                throw new InvalidOperationException("Stages must be defined and cannot be empty.");
            }
            _currentStageIndex = 0;
        }

        public TStage CurrentStage
        {
            get => Stages[_currentStageIndex];
            set
            {
                int index = Array.IndexOf(Stages, value);
                if (index >= 0)
                {
                    _currentStageIndex = index;
                }
            }
        }

        public abstract TStage MaxStageReached { get; set; }

        public abstract bool CanIncrementStage { get; }

        public bool CanDecrementStage => _currentStageIndex > 0;

        public bool IsFirstStage => _currentStageIndex == 0;

        public bool IsLastStage => _currentStageIndex == Stages.Length - 1;

        public void NextStage()
        {
            while (CanIncrementStage)
            {
                _currentStageIndex++;
                MaxStageReached = Stages[_currentStageIndex];
            }
        }

        public void PreviousStage()
        {
            if (CanDecrementStage)
            {
                _currentStageIndex--;
            }
        }

        public bool GoToStage(TStage stage)
        {
            int targetIndex = Array.IndexOf(Stages, stage);

            if (targetIndex >= 0 && targetIndex <= Array.IndexOf(Stages, MaxStageReached))
            {
                _currentStageIndex = targetIndex;
                return true;
            }

            return false;
        }

        public TStage? GetNextStage()
        {
            return _currentStageIndex < Stages.Length - 1
                ? Stages[_currentStageIndex + 1]
                : null;
        }

        public TStage? GetPreviousStage()
        {
            return _currentStageIndex > 0
                ? Stages[_currentStageIndex - 1]
                : null;
        }
    }
}