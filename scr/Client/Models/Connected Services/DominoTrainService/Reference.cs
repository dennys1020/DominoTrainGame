
namespace DominoTrainGame.Client.Models.DominoTrainService {
    using System.Runtime.Serialization;
    using System;


    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="User", Namespace="http://schemas.datacontract.org/2004/07/DominoTrainGame.Server.Service.DTO")]
    [System.SerializableAttribute()]
    public partial class User : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {

        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string EmailField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string LanguageField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string PasswordField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string UsernameField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Guid VerificationIdField;

        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Email {
            get {
                return this.EmailField;
            }
            set {
                if ((object.ReferenceEquals(this.EmailField, value) != true)) {
                    this.EmailField = value;
                    this.RaisePropertyChanged("Email");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Language {
            get {
                return this.LanguageField;
            }
            set {
                if ((object.ReferenceEquals(this.LanguageField, value) != true)) {
                    this.LanguageField = value;
                    this.RaisePropertyChanged("Language");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Password {
            get {
                return this.PasswordField;
            }
            set {
                if ((object.ReferenceEquals(this.PasswordField, value) != true)) {
                    this.PasswordField = value;
                    this.RaisePropertyChanged("Password");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Username {
            get {
                return this.UsernameField;
            }
            set {
                if ((object.ReferenceEquals(this.UsernameField, value) != true)) {
                    this.UsernameField = value;
                    this.RaisePropertyChanged("Username");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Guid VerificationId {
            get {
                return this.VerificationIdField;
            }
            set {
                if ((this.VerificationIdField.Equals(value) != true)) {
                    this.VerificationIdField = value;
                    this.RaisePropertyChanged("VerificationId");
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="LoginStatus", Namespace="http://schemas.datacontract.org/2004/07/DominoTrainGame.Server.Service.DTO")]
    public enum LoginStatus : int {

        [System.Runtime.Serialization.EnumMemberAttribute()]
        SystemError = 0,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        Success = 1,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        EmptyFields = 2,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        UserNotFound = 3,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        IncorrectPassword = 4,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="RegistrationStatus", Namespace="http://schemas.datacontract.org/2004/07/DominoTrainGame.Server.Service.DTO")]
    public enum RegistrationStatus : int {

        [System.Runtime.Serialization.EnumMemberAttribute()]
        SystemError = 0,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        Success = 1,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        EmptyFields = 2,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        InvalidEmail = 3,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        PasswordTooShort = 4,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        UserAlreadyExists = 5,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        VerificationRequired = 6,
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="BeginVerificationRequest", Namespace="http://schemas.datacontract.org/2004/07/DominoTrainGame.Server.Service.DTO")]
    [System.SerializableAttribute()]
    public partial class BeginVerificationRequest : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {

        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string EmailField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string LanguageField;

        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Email {
            get {
                return this.EmailField;
            }
            set {
                if ((object.ReferenceEquals(this.EmailField, value) != true)) {
                    this.EmailField = value;
                    this.RaisePropertyChanged("Email");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Language {
            get {
                return this.LanguageField;
            }
            set {
                if ((object.ReferenceEquals(this.LanguageField, value) != true)) {
                    this.LanguageField = value;
                    this.RaisePropertyChanged("Language");
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="VerificationSessionDto", Namespace="http://schemas.datacontract.org/2004/07/DominoTrainGame.Server.Service.DTO")]
    [System.SerializableAttribute()]
    public partial class VerificationSessionDto : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {

        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Guid SessionIdField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool WasSentField;

        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Guid SessionId {
            get {
                return this.SessionIdField;
            }
            set {
                if ((this.SessionIdField.Equals(value) != true)) {
                    this.SessionIdField = value;
                    this.RaisePropertyChanged("SessionId");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool WasSent {
            get {
                return this.WasSentField;
            }
            set {
                if ((this.WasSentField.Equals(value) != true)) {
                    this.WasSentField = value;
                    this.RaisePropertyChanged("WasSent");
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="VerificationStatus", Namespace="http://schemas.datacontract.org/2004/07/DominoTrainGame.Server.Service.DTO")]
    public enum VerificationStatus : int {

        [System.Runtime.Serialization.EnumMemberAttribute()]
        SystemError = 0,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        Valid = 1,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        Invalid = 2,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        Expired = 3,

        [System.Runtime.Serialization.EnumMemberAttribute()]
        TooManyAttempts = 4,
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="LeaderboardResponse", Namespace="http://schemas.datacontract.org/2004/07/DominoTrainGame.Server.Service.DTO")]
    [System.SerializableAttribute()]
    public partial class LeaderboardResponse : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {

        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private DominoTrainGame.Client.Models.DominoTrainService.LeaderboardEntry[] EntriesField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool SucceededField;

        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public DominoTrainGame.Client.Models.DominoTrainService.LeaderboardEntry[] Entries {
            get {
                return this.EntriesField;
            }
            set {
                if ((object.ReferenceEquals(this.EntriesField, value) != true)) {
                    this.EntriesField = value;
                    this.RaisePropertyChanged("Entries");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool Succeeded {
            get {
                return this.SucceededField;
            }
            set {
                if ((this.SucceededField.Equals(value) != true)) {
                    this.SucceededField = value;
                    this.RaisePropertyChanged("Succeeded");
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="LeaderboardEntry", Namespace="http://schemas.datacontract.org/2004/07/DominoTrainGame.Server.Service.DTO")]
    [System.SerializableAttribute()]
    public partial class LeaderboardEntry : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {

        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int GamesPlayedField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int PlayerIdField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int RankField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private long TotalScoreField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string UsernameField;

        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int WinsField;

        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public int GamesPlayed {
            get {
                return this.GamesPlayedField;
            }
            set {
                if ((this.GamesPlayedField.Equals(value) != true)) {
                    this.GamesPlayedField = value;
                    this.RaisePropertyChanged("GamesPlayed");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public int PlayerId {
            get {
                return this.PlayerIdField;
            }
            set {
                if ((this.PlayerIdField.Equals(value) != true)) {
                    this.PlayerIdField = value;
                    this.RaisePropertyChanged("PlayerId");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public int Rank {
            get {
                return this.RankField;
            }
            set {
                if ((this.RankField.Equals(value) != true)) {
                    this.RankField = value;
                    this.RaisePropertyChanged("Rank");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public long TotalScore {
            get {
                return this.TotalScoreField;
            }
            set {
                if ((this.TotalScoreField.Equals(value) != true)) {
                    this.TotalScoreField = value;
                    this.RaisePropertyChanged("TotalScore");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Username {
            get {
                return this.UsernameField;
            }
            set {
                if ((object.ReferenceEquals(this.UsernameField, value) != true)) {
                    this.UsernameField = value;
                    this.RaisePropertyChanged("Username");
                }
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public int Wins {
            get {
                return this.WinsField;
            }
            set {
                if ((this.WinsField.Equals(value) != true)) {
                    this.WinsField = value;
                    this.RaisePropertyChanged("Wins");
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="DominoTrainService.IAuthenticationService", CallbackContract=typeof(DominoTrainGame.Client.Models.DominoTrainService.IAuthenticationServiceCallback))]
    public interface IAuthenticationService {

        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IAuthenticationService/LogIn")]
        void LogIn(System.Guid requestId, string identifier, string password);

        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IAuthenticationService/LogIn")]
        System.Threading.Tasks.Task LogInAsync(System.Guid requestId, string identifier, string password);

        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IAuthenticationService/RegisterUser")]
        void RegisterUser(System.Guid requestId, DominoTrainGame.Client.Models.DominoTrainService.User user);

        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IAuthenticationService/RegisterUser")]
        System.Threading.Tasks.Task RegisterUserAsync(System.Guid requestId, DominoTrainGame.Client.Models.DominoTrainService.User user);

        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IAuthenticationService/CheckAvailability")]
        void CheckAvailability(System.Guid requestId, DominoTrainGame.Client.Models.DominoTrainService.User user);

        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IAuthenticationService/CheckAvailability")]
        System.Threading.Tasks.Task CheckAvailabilityAsync(System.Guid requestId, DominoTrainGame.Client.Models.DominoTrainService.User user);
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IAuthenticationServiceCallback {

        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IAuthenticationService/OnLogInCompleted")]
        void OnLogInCompleted(System.Guid requestId, DominoTrainGame.Client.Models.DominoTrainService.LoginStatus status);

        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IAuthenticationService/OnRegistrationCompleted")]
        void OnRegistrationCompleted(System.Guid requestId, DominoTrainGame.Client.Models.DominoTrainService.RegistrationStatus status);

        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IAuthenticationService/OnAvailabilityChecked")]
        void OnAvailabilityChecked(System.Guid requestId, DominoTrainGame.Client.Models.DominoTrainService.RegistrationStatus status);
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IAuthenticationServiceChannel : DominoTrainGame.Client.Models.DominoTrainService.IAuthenticationService, System.ServiceModel.IClientChannel {
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class AuthenticationServiceClient : System.ServiceModel.DuplexClientBase<DominoTrainGame.Client.Models.DominoTrainService.IAuthenticationService>, DominoTrainGame.Client.Models.DominoTrainService.IAuthenticationService {

        public AuthenticationServiceClient(System.ServiceModel.InstanceContext callbackInstance) :
                base(callbackInstance) {
        }

        public AuthenticationServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName) :
                base(callbackInstance, endpointConfigurationName) {
        }

        public AuthenticationServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) :
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }

        public AuthenticationServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) :
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }

        public AuthenticationServiceClient(System.ServiceModel.InstanceContext callbackInstance, System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
                base(callbackInstance, binding, remoteAddress) {
        }

        public void LogIn(System.Guid requestId, string identifier, string password) {
            base.Channel.LogIn(requestId, identifier, password);
        }

        public System.Threading.Tasks.Task LogInAsync(System.Guid requestId, string identifier, string password) {
            return base.Channel.LogInAsync(requestId, identifier, password);
        }

        public void RegisterUser(System.Guid requestId, DominoTrainGame.Client.Models.DominoTrainService.User user) {
            base.Channel.RegisterUser(requestId, user);
        }

        public System.Threading.Tasks.Task RegisterUserAsync(System.Guid requestId, DominoTrainGame.Client.Models.DominoTrainService.User user) {
            return base.Channel.RegisterUserAsync(requestId, user);
        }

        public void CheckAvailability(System.Guid requestId, DominoTrainGame.Client.Models.DominoTrainService.User user) {
            base.Channel.CheckAvailability(requestId, user);
        }

        public System.Threading.Tasks.Task CheckAvailabilityAsync(System.Guid requestId, DominoTrainGame.Client.Models.DominoTrainService.User user) {
            return base.Channel.CheckAvailabilityAsync(requestId, user);
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="DominoTrainService.IVerificationService", CallbackContract=typeof(DominoTrainGame.Client.Models.DominoTrainService.IVerificationServiceCallback))]
    public interface IVerificationService {

        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IVerificationService/BeginVerification")]
        void BeginVerification(System.Guid requestId, DominoTrainGame.Client.Models.DominoTrainService.BeginVerificationRequest request);

        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IVerificationService/BeginVerification")]
        System.Threading.Tasks.Task BeginVerificationAsync(System.Guid requestId, DominoTrainGame.Client.Models.DominoTrainService.BeginVerificationRequest request);

        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IVerificationService/ResendVerification")]
        void ResendVerification(System.Guid requestId, System.Guid sessionId);

        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IVerificationService/ResendVerification")]
        System.Threading.Tasks.Task ResendVerificationAsync(System.Guid requestId, System.Guid sessionId);

        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IVerificationService/VerifyCode")]
        void VerifyCode(System.Guid requestId, System.Guid sessionId, string code);

        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IVerificationService/VerifyCode")]
        System.Threading.Tasks.Task VerifyCodeAsync(System.Guid requestId, System.Guid sessionId, string code);
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IVerificationServiceCallback {

        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IVerificationService/OnVerificationStarted")]
        void OnVerificationStarted(System.Guid requestId, DominoTrainGame.Client.Models.DominoTrainService.VerificationSessionDto session);

        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IVerificationService/OnVerificationResent")]
        void OnVerificationResent(System.Guid requestId, bool wasSent);

        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IVerificationService/OnCodeVerified")]
        void OnCodeVerified(System.Guid requestId, DominoTrainGame.Client.Models.DominoTrainService.VerificationStatus status);
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IVerificationServiceChannel : DominoTrainGame.Client.Models.DominoTrainService.IVerificationService, System.ServiceModel.IClientChannel {
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class VerificationServiceClient : System.ServiceModel.DuplexClientBase<DominoTrainGame.Client.Models.DominoTrainService.IVerificationService>, DominoTrainGame.Client.Models.DominoTrainService.IVerificationService {

        public VerificationServiceClient(System.ServiceModel.InstanceContext callbackInstance) :
                base(callbackInstance) {
        }

        public VerificationServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName) :
                base(callbackInstance, endpointConfigurationName) {
        }

        public VerificationServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) :
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }

        public VerificationServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) :
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }

        public VerificationServiceClient(System.ServiceModel.InstanceContext callbackInstance, System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
                base(callbackInstance, binding, remoteAddress) {
        }

        public void BeginVerification(System.Guid requestId, DominoTrainGame.Client.Models.DominoTrainService.BeginVerificationRequest request) {
            base.Channel.BeginVerification(requestId, request);
        }

        public System.Threading.Tasks.Task BeginVerificationAsync(System.Guid requestId, DominoTrainGame.Client.Models.DominoTrainService.BeginVerificationRequest request) {
            return base.Channel.BeginVerificationAsync(requestId, request);
        }

        public void ResendVerification(System.Guid requestId, System.Guid sessionId) {
            base.Channel.ResendVerification(requestId, sessionId);
        }

        public System.Threading.Tasks.Task ResendVerificationAsync(System.Guid requestId, System.Guid sessionId) {
            return base.Channel.ResendVerificationAsync(requestId, sessionId);
        }

        public void VerifyCode(System.Guid requestId, System.Guid sessionId, string code) {
            base.Channel.VerifyCode(requestId, sessionId, code);
        }

        public System.Threading.Tasks.Task VerifyCodeAsync(System.Guid requestId, System.Guid sessionId, string code) {
            return base.Channel.VerifyCodeAsync(requestId, sessionId, code);
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="urn:domino-train:services:v1", ConfigurationName="DominoTrainService.ILeaderboardService", CallbackContract=typeof(DominoTrainGame.Client.Models.DominoTrainService.ILeaderboardServiceCallback))]
    public interface ILeaderboardService {

        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="urn:domino-train:services:v1/ILeaderboardService/GetLeaderboard")]
        void GetLeaderboard(System.Guid requestId);

        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="urn:domino-train:services:v1/ILeaderboardService/GetLeaderboard")]
        System.Threading.Tasks.Task GetLeaderboardAsync(System.Guid requestId);
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ILeaderboardServiceCallback {

        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="urn:domino-train:services:v1/ILeaderboardService/OnLeaderboardReceived")]
        void OnLeaderboardReceived(System.Guid requestId, DominoTrainGame.Client.Models.DominoTrainService.LeaderboardResponse response);
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ILeaderboardServiceChannel : DominoTrainGame.Client.Models.DominoTrainService.ILeaderboardService, System.ServiceModel.IClientChannel {
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class LeaderboardServiceClient : System.ServiceModel.DuplexClientBase<DominoTrainGame.Client.Models.DominoTrainService.ILeaderboardService>, DominoTrainGame.Client.Models.DominoTrainService.ILeaderboardService {

        public LeaderboardServiceClient(System.ServiceModel.InstanceContext callbackInstance) :
                base(callbackInstance) {
        }

        public LeaderboardServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName) :
                base(callbackInstance, endpointConfigurationName) {
        }

        public LeaderboardServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) :
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }

        public LeaderboardServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) :
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }

        public LeaderboardServiceClient(System.ServiceModel.InstanceContext callbackInstance, System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
                base(callbackInstance, binding, remoteAddress) {
        }

        public void GetLeaderboard(System.Guid requestId) {
            base.Channel.GetLeaderboard(requestId);
        }

        public System.Threading.Tasks.Task GetLeaderboardAsync(System.Guid requestId) {
            return base.Channel.GetLeaderboardAsync(requestId);
        }
    }
}
