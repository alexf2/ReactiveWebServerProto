using System;
using System.Xml.Linq;
using AnywayAnyday.GuestBook.Contract.DTO;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using AutoMapper;

namespace AnywayAnyday.DataProviders.GuestBookXmlProvider
{
    public sealed class AssemblyInstaller: IWindsorInstaller
    {
        public void Install (IWindsorContainer container, IConfigurationStore store)
        {            
            Mapper.Initialize(cfg =>
            {                
                Mapper.CreateMap<UserInfo, XElement>().ConstructUsing(
                    u =>new XElement("user", new XAttribute("login", u.UserLogin), new XAttribute("display", u.DisplayName), new XAttribute("created", u.Created))                        
                );

                Mapper.CreateMap<UserMessage, XElement>().ConstructUsing(
                    u => new XElement("message", new XAttribute("created", u.Created), u.Text)
                );

                Mapper.CreateMap<XElement, UserInfo>()
                    .ForMember(dst => dst.UserLogin, opt => opt.MapFrom(src => src.Attribute("login").Value))
                    .ForMember(dst => dst.DisplayName,
                        opt => opt.ResolveUsing<XAttributeResolver<string>>().FromMember(src => src.Attribute("display")))
                    .ForMember(dst => dst.Created, opt => opt.MapFrom(src => (DateTimeOffset)src.Attribute("created")));


                Mapper.CreateMap<XElement, UserMessage>()
                    .ForMember(dst => dst.Text, opt => opt.MapFrom(src => src.Value))
                    .ForMember(dst => dst.UserLogin, opt => opt.MapFrom(src => src.Parent.Attribute("login").Value))
                    .ForMember(dst => dst.Created, opt => opt.MapFrom(src => (DateTimeOffset)src.Attribute("created")));
            });

            Mapper.AssertConfigurationIsValid();
        }

    }
}
