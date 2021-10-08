﻿using PostSharp.Aspects;
using PostSharp.Extensibility;
using PostSharp.Serialization;
using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PostSharpBlazorSyncfusionGrid.Aspects
{
  /// <summary>
  ///   Aspect that, when applied to a method, causes invocations of this method to be retried if the method ends with
  ///   specified exceptions.
  /// </summary>
  [PSerializable]
  [LinesOfCodeAvoided(5)]
  [MulticastAttributeUsage(MulticastTargets.Method)]
  public sealed class AutoRetryAttribute : MethodInterceptionAspect
  {
    /// <summary>
    ///   Initializes a new <see cref="AutoRetryAttribute" /> with default values.
    /// </summary>
    public AutoRetryAttribute()
    {
      // Set the default values for properties.
      MaxRetries = 5;
      Delay = 3;
      HandledExceptions = new[] { typeof(Exception), typeof(DataException) };
    }

    /// <summary>
    ///   Gets or sets the maximum number of retries. The default value is 5.
    /// </summary>
    public int MaxRetries
    {
      get; set;
    }

    /// <summary>
    ///   Gets or sets the delay before retrying, in seconds. The default value is 3.
    /// </summary>
    public float Delay
    {
      get; set;
    }

    /// <summary>
    ///   Gets or sets the type of exceptions that cause the method invocation to be retried. The default value is
    ///   <see cref="WebException" /> and <see cref="DataException" />.
    /// </summary>
    public Type[] HandledExceptions
    {
      get; set;
    }

    public override async Task OnInvokeAsync(MethodInterceptionArgs args)
    {
      for (var i = 0; ; i++)
      {
        try
        {
          // Invoke the intercepted method.
          await args.ProceedAsync();

          // If we get here, it means the execution was successful.
          return;
        }
        catch (Exception e)
        {
          // The intercepted method threw an exception. Figure out if we can retry the method.

          if (CanRetry(i, e))
          {
            // Yes, we can retry. Write some message and wait a bit.

            Console.WriteLine(
              $"Method failed with exception {e.GetType().Name} '{e.Message}'. Sleeping {Delay} s and retrying. This was our attempt #{i + 1}.");

            if (Delay > 0)
            {
              await Task.Delay(TimeSpan.FromSeconds(Delay));
            }

            // Continue to the next iteration.
          }
          else
          {
            // No, we cannot retry. Rethrow the exception.
            throw;
          }
        }
      }
    }

    private bool CanRetry(int attempt, Exception e)
    {
      return attempt < MaxRetries &&
             (HandledExceptions == null || HandledExceptions.Any(type => type.IsInstanceOfType(e)));
    }
  }
}