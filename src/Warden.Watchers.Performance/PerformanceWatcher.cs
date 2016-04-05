﻿using System;
using System.Threading.Tasks;

namespace Warden.Watchers.Performance
{
    /// <summary>
    /// PerformanceWatcher designed for CPU & RAM monitoring.
    /// </summary>
    public class PerformanceWatcher : IWatcher
    {
        private readonly PerformanceWatcherConfiguration _configuration;
        public string Name { get; }
        public const string DefaultName = "Performance Watcher";

        protected PerformanceWatcher(string name, PerformanceWatcherConfiguration configuration)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Watcher name can not be empty.");

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration),
                    "Performance Watcher configuration has not been provided.");
            }

            Name = name;
            _configuration = configuration;
        }

        public async Task<IWatcherCheckResult> ExecuteAsync()
        {
            try
            {
                var resourceUsage = await _configuration.PerformanceProvider().GetResourceUsageAsync();
                var isValid = true;
                if (_configuration.EnsureThatAsync != null)
                    isValid = await _configuration.EnsureThatAsync?.Invoke(resourceUsage);

                isValid = isValid && (_configuration.EnsureThat?.Invoke(resourceUsage) ?? true);

                return PerformanceWatcherCheckResult.Create(this, isValid, _configuration.Delay, resourceUsage,
                    $"CPU usage: {resourceUsage.Cpu.ToString("F")}%, RAM usage: {resourceUsage.Ram} MB.");
            }
            catch (Exception exception)
            {
                throw new WatcherException("There was an error while trying to calculate performance.", exception);
            }
        }

        /// <summary>
        /// Factory method for creating a new instance of PerformanceWatcher with default name of Performance Watcher.
        /// </summary>
        /// <param name="delay">Delay between resource usage calculation while using the default performance counter (100 ms by default).</param>
        /// <param name="configurator">Optional lambda expression for configuring the PerformanceWatcher.</param>
        /// <returns>Instance of PerformanceWatcher.</returns>
        public static PerformanceWatcher Create(TimeSpan? delay = null,
            Action<PerformanceWatcherConfiguration.Default> configurator = null)
            => Create(DefaultName, delay, configurator);

        /// <summary>
        /// Factory method for creating a new instance of PerformanceWatcher with default name of Performance Watcher.
        /// </summary>
        /// <param name="name">Name of the PerformanceWatcher.</param>
        /// <param name="delay">Delay between resource usage calculation while using the default performance counter (100 ms by default).</param>
        /// <param name="configurator">Optional lambda expression for configuring the PerformanceWatcher.</param>
        /// <returns>Instance of PerformanceWatcher.</returns>
        public static PerformanceWatcher Create(string name, TimeSpan? delay = null,
            Action<PerformanceWatcherConfiguration.Default> configurator = null)
        {
            var config = new PerformanceWatcherConfiguration.Builder(delay);
            configurator?.Invoke((PerformanceWatcherConfiguration.Default) config);

            return Create(name, config.Build());
        }

        /// <summary>
        /// Factory method for creating a new instance of PerformanceWatcher with default name of Performance Watcher.
        /// </summary>
        /// <param name="configuration">Configuration of PerformanceWatcher.</param>
        /// <returns>Instance of PerformanceWatcher.</returns>
        public static PerformanceWatcher Create(PerformanceWatcherConfiguration configuration)
            => Create(DefaultName, configuration);

        /// <summary>
        /// Factory method for creating a new instance of PerformanceWatcher.
        /// </summary>
        /// <param name="name">Name of the PerformanceWatcher.</param>
        /// <param name="configuration">Configuration of PerformanceWatcher.</param>
        /// <returns>Instance of PerformanceWatcher.</returns>
        public static PerformanceWatcher Create(string name, PerformanceWatcherConfiguration configuration)
            => new PerformanceWatcher(name, configuration);
    }
}