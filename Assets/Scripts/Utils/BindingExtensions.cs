﻿using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Modules.MapGenerator;
using UnityEngine;

namespace Utils
{
    public static class BindingExtensions
    {
        public static void BindMapDataToMainTex(this IUniTaskAsyncEnumerable<RawMapData> source, Renderer target,
            CancellationToken cancellationToken, bool rebindOnError = true)
        {
            void Setter(RawMapData value)
            {
                if (value != null)
                {
                    target.material.mainTexture = value.GetTexture();
                }
            }

            BindToCore(source, Setter, cancellationToken, rebindOnError).Forget();
        }

        public static void BindToScale(this IUniTaskAsyncEnumerable<Resolution> source, Transform transform,
            CancellationToken cancellationToken, bool rebindOnError = true)
        {
            void Setter(Resolution value)
            {
                if (value.width != 0 && value.height != 0)
                    transform.localScale = new Vector3(value.width / (float)value.height, 1.0f, 1.0f);
            }

            BindToCore(source, Setter, cancellationToken, rebindOnError).Forget();
        }


        public static async UniTaskVoid BindToCore<TValue>(IUniTaskAsyncEnumerable<TValue> source,
            Action<TValue> setter, CancellationToken cancellationToken, bool rebindOnError)
        {
            var repeat = false;
            BIND_AGAIN:
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (true)
                {
                    bool moveNext;
                    try
                    {
                        moveNext = await e.MoveNextAsync();
                        repeat = false;
                    }
                    catch (Exception ex)
                    {
                        if (ex is OperationCanceledException) return;

                        if (rebindOnError && !repeat)
                        {
                            repeat = true;
                            goto BIND_AGAIN;
                        }
                        else
                        {
                            throw;
                        }
                    }

                    if (!moveNext) return;

                    setter(e.Current);
                }
            }
            finally
            {
                if (e != null) await e.DisposeAsync();
            }
        }
    }
}