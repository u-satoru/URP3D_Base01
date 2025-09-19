using UnityEngine;
using System.Linq;
using asterivo.Unity60.Features.Templates.Common;

namespace asterivo.Unity60.Features.Templates.Common
{
    /// <summary>
    /// Base class for all template managers providing common functionality
    /// for template initialization, activation, and management.
    /// </summary>
    public abstract class BaseTemplateManager : MonoBehaviour
    {
        [Header("Template Configuration")]
        [SerializeField] protected GenreTemplateConfig templateConfig;

        [Header("Template State")]
        [SerializeField] protected bool isInitialized = false;
        [SerializeField] protected bool isActive = false;

        #region Abstract Properties

        /// <summary>
        /// The genre type this template manages
        /// </summary>
        public abstract GenreType GenreType { get; }

        /// <summary>
        /// Display name of this template
        /// </summary>
        public abstract string TemplateName { get; }

        /// <summary>
        /// Description of what this template provides
        /// </summary>
        public abstract string TemplateDescription { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets whether this template has been initialized
        /// </summary>
        public bool IsInitialized => isInitialized;

        /// <summary>
        /// Gets whether this template is currently active
        /// </summary>
        public bool IsActive => isActive;

        /// <summary>
        /// Gets the template configuration
        /// </summary>
        public GenreTemplateConfig TemplateConfig => templateConfig;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            if (templateConfig == null)
            {
                Debug.LogWarning($"{TemplateName}: Template configuration is not assigned.");
            }
        }

        protected virtual void Start()
        {
            if (!isInitialized)
            {
                InitializeTemplate();
            }
        }

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Initialize the template with default settings
        /// </summary>
        public virtual void InitializeTemplate()
        {
            if (isInitialized)
            {
                Debug.LogWarning($"{TemplateName}: Template is already initialized.");
                return;
            }

            Debug.Log($"Initializing {TemplateName} template...");

            // Perform base initialization
            PerformBaseInitialization();

            // Allow derived classes to perform specific initialization
            OnInitializeTemplate();

            isInitialized = true;
            Debug.Log($"{TemplateName} template initialized successfully.");
        }

        /// <summary>
        /// Called when the template is activated
        /// </summary>
        public virtual void OnTemplateActivated()
        {
            if (!isInitialized)
            {
                Debug.LogWarning($"{TemplateName}: Cannot activate template that is not initialized.");
                return;
            }

            if (isActive)
            {
                Debug.LogWarning($"{TemplateName}: Template is already active.");
                return;
            }

            Debug.Log($"Activating {TemplateName} template...");
            isActive = true;

            // Allow derived classes to handle activation
            OnActivateTemplate();
        }

        /// <summary>
        /// Called when the template is deactivated
        /// </summary>
        public virtual void OnTemplateDeactivated()
        {
            if (!isActive)
            {
                Debug.LogWarning($"{TemplateName}: Template is not currently active.");
                return;
            }

            Debug.Log($"Deactivating {TemplateName} template...");
            isActive = false;

            // Allow derived classes to handle deactivation
            OnDeactivateTemplate();
        }

        /// <summary>
        /// Validate the template configuration and state
        /// </summary>
        public virtual bool ValidateTemplate()
        {
            bool isValid = true;

            // Basic validation
            if (templateConfig == null)
            {
                Debug.LogError($"{TemplateName}: Template configuration is missing.");
                isValid = false;
            }

            // Allow derived classes to perform additional validation
            if (!OnValidateTemplate())
            {
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// Get template information for display and management
        /// </summary>
        public virtual TemplateInfo GetTemplateInfo()
        {
            var info = new TemplateInfo(TemplateName, TemplateDescription, GenreType);

            // Set additional information from config if available
            if (templateConfig != null)
            {
                info.Version = templateConfig.Version;
                info.RequiredComponents = templateConfig.RequiredComponents.ToArray();
                info.OptionalComponents = templateConfig.OptionalComponents.ToArray();
                info.SpecialFeatures = templateConfig.SpecialFeatures.ToArray();
                info.RequiresAI = templateConfig.RequiresAI();
                info.RequiresAudio = templateConfig.RequiresAudio;
                info.RequiresNetworking = templateConfig.RequiresNetworking;
                info.MaxNPCCount = templateConfig.MaxNPCCount;
                info.EstimatedLearningTimeMinutes = templateConfig.GetEstimatedLearningTimeMinutes();
                info.EstimatedGameplayMinutes = templateConfig.EstimatedGameplayMinutes;
                info.Priority = templateConfig.GetPriority();
            }

            // Allow derived classes to add specific information
            OnPopulateTemplateInfo(info);

            return info;
        }

        #endregion

        #region Protected Abstract/Virtual Methods

        /// <summary>
        /// Override this method to provide template-specific initialization
        /// </summary>
        protected abstract void OnInitializeTemplate();

        /// <summary>
        /// Override this method to handle template activation
        /// </summary>
        protected virtual void OnActivateTemplate()
        {
            // Default implementation - derived classes can override
        }

        /// <summary>
        /// Override this method to handle template deactivation
        /// </summary>
        protected virtual void OnDeactivateTemplate()
        {
            // Default implementation - derived classes can override
        }

        /// <summary>
        /// Override this method to provide template-specific validation
        /// </summary>
        /// <returns>True if validation passes, false otherwise</returns>
        protected virtual bool OnValidateTemplate()
        {
            // Default implementation returns true - derived classes can override
            return true;
        }

        /// <summary>
        /// Override this method to add template-specific information
        /// </summary>
        /// <param name="info">Template info to populate</param>
        protected virtual void OnPopulateTemplateInfo(TemplateInfo info)
        {
            // Default implementation - derived classes can override
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Perform common initialization tasks for all templates
        /// </summary>
        private void PerformBaseInitialization()
        {
            // Basic setup that all templates need
            gameObject.name = $"{TemplateName}TemplateManager";
        }

        #endregion

        #region Debug Methods

        /// <summary>
        /// Get debug information about this template
        /// </summary>
        public string GetDebugInfo()
        {
            return $"Template: {TemplateName}\n" +
                   $"Genre: {GenreType}\n" +
                   $"Initialized: {isInitialized}\n" +
                   $"Active: {isActive}\n" +
                   $"Config: {(templateConfig != null ? "Assigned" : "Missing")}\n" +
                   $"Valid: {ValidateTemplate()}";
        }

        #endregion
    }
}