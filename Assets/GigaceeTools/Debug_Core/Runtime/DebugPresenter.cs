﻿using UniRx;
using UnityEngine;

namespace GigaceeTools
{
    [DefaultExecutionOrder(-1)]
    public class DebugPresenter : MonoBehaviour
    {
        [SerializeField] private BoolReactiveProperty _debugMode;
        [SerializeField] private bool _forceReleaseBuild;

        private IDebugCore _debugCore;

        private void Awake()
        {
            if (!Debug.isDebugBuild)
            {
                Destroy(this);
                return;
            }

            if (ServiceLocator.TryGetInstance(out _debugCore))
            {
                if (FindObjectsOfType<DebugPresenter>(true).Length == 1)
                {
                    LinkDebugModeFlags(_debugCore);
                    return;
                }

                Destroy(this);
                return;
            }

            if (_forceReleaseBuild)
            {
                Destroy(this);
                return;
            }

            _debugCore = new DebugCore(_debugMode.Value);

            LinkDebugModeFlags(_debugCore);

            ServiceLocator.Register(_debugCore);
        }

        private void OnApplicationQuit()
        {
            ServiceLocator.Unregister(_debugCore);
        }

        /// <summary>
        /// この Presenter のデバッグモードフラグと、DebugCore のデバッグモードフラグを連動させます。
        /// </summary>
        /// <param name="debugCore"></param>
        private void LinkDebugModeFlags(IDebugCore debugCore)
        {
            debugCore
                .IsDebugMode
                .Subscribe(x =>
                {
                    _debugMode.Value = x;
                })
                .AddTo(this);

            _debugMode
                .SkipLatestValueOnSubscribe()
                .Subscribe(x =>
                {
                    debugCore.IsDebugMode.Value = x;
                })
                .AddTo(this);
        }
    }
}
