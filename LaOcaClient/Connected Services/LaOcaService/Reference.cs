﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LaOcaClient.LaOcaService {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="LaOcaService.ILoginService")]
    public interface ILoginService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ILoginService/GetUser", ReplyAction="http://tempuri.org/ILoginService/GetUserResponse")]
        int GetUser(string username);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ILoginService/GetUser", ReplyAction="http://tempuri.org/ILoginService/GetUserResponse")]
        System.Threading.Tasks.Task<int> GetUserAsync(string username);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ILoginServiceChannel : LaOcaClient.LaOcaService.ILoginService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class LoginServiceClient : System.ServiceModel.ClientBase<LaOcaClient.LaOcaService.ILoginService>, LaOcaClient.LaOcaService.ILoginService {
        
        public LoginServiceClient() {
        }
        
        public LoginServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public LoginServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public LoginServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public LoginServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public int GetUser(string username) {
            return base.Channel.GetUser(username);
        }
        
        public System.Threading.Tasks.Task<int> GetUserAsync(string username) {
            return base.Channel.GetUserAsync(username);
        }
    }
}
