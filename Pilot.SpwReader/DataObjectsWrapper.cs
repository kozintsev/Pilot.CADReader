using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ascon.Pilot.SDK.CadReader
{
    internal class DataObjectWrapper
    {
        public DataObjectWrapper(IDataObject dataObject, IObjectsRepository repository)
        {
            if (dataObject == null)
                return;

            Id = dataObject.Id;
            ParentId = dataObject.ParentId;
            Created = dataObject.Created;
            Attributes = new Dictionary<string, object>(dataObject.Attributes);
            DisplayName = dataObject.DisplayName;
            Type = new TypeWrapper(dataObject.Type);
            Creator = new PersonWrapper(dataObject.Creator);
            ListViewChildren = new ReadOnlyCollection<Guid>(
                ChildrenFilters.GetChildrenForListView(dataObject, repository).ToList());
            PilotStorageChildren = new ReadOnlyCollection<Guid>(
                ChildrenFilters.GetChildrenForPilotStorage(dataObject, repository).ToList());
            State = dataObject.State;
            SynchronizationState = dataObject.SynchronizationState;
            Files = new ReadOnlyCollection<FileWrapper>(dataObject.Files.Select(f => new FileWrapper(f)).ToList());
            Access = new ReadOnlyCollection<AccessWrapper>(dataObject.Access2.Select(f => new AccessWrapper(f)).ToList());
            IsSecret = dataObject.IsSecret;
            StateInfo = new ObjectStateInfo(dataObject.ObjectStateInfo);
        }

        public Guid Id { get; private set; }
        public Guid ParentId { get; private set; }
        public DateTime Created { get; private set; }
        public IDictionary<string, object> Attributes { get; private set; }
        public string DisplayName { get; private set; }
        public TypeWrapper Type { get; private set; }
        public PersonWrapper Creator { get; private set; }
        public ReadOnlyCollection<Guid> ListViewChildren { get; private set; }
        public ReadOnlyCollection<Guid> PilotStorageChildren { get; private set; }
        public DataState State { get; private set; }
        public SynchronizationState SynchronizationState { get; private set; }
        public ReadOnlyCollection<FileWrapper> Files { get; private set; }
        public ReadOnlyCollection<AccessWrapper> Access { get; private set; }
        public bool IsSecret { get; private set; }
        public IStateInfo StateInfo { get; private set; }
    }

    internal class ObjectStateInfo : IStateInfo
    {
        public ObjectStateInfo(IStateInfo objectStateInfo)
        {
            State = objectStateInfo.State;
            Date = objectStateInfo.Date;
            PersonId = objectStateInfo.PersonId;
            PositionId = objectStateInfo.PositionId;
        }

        public ObjectState State { get; private set; }
        public DateTime Date { get; private set; }
        public int PersonId { get; private set; }
        public int PositionId { get; private set; }
    }

    internal class FileWrapper
    {
        public FileWrapper(IFile file)
        {
            if (file == null) 
                return;

            Id = file.Id;
            Size = file.Size;
            Md5 = file.Md5;
            Name = file.Name;
            Modified = file.Modified;
            Created = file.Created;
            Accessed = file.Accessed;
            Signatures = new ReadOnlyCollection<SignatureWrapper>(file.Signatures.Select(s => new SignatureWrapper(s)).ToList());
        }

        public Guid Id { get; private set; }
        public long Size { get; private set; }
        public string Md5 { get; private set; }
        public string Name { get; private set; }
        public DateTime Modified { get; private set; }
        public DateTime Created { get; private set; }
        public DateTime Accessed { get; private set; }
        public ReadOnlyCollection<SignatureWrapper> Signatures { get; private set; }
    }

    internal class SignatureWrapper
    {
        public SignatureWrapper(ISignature signature)
        {
            if (signature == null) 
                return;

            Id = signature.Id;
            DatabaseId = signature.DatabaseId;
            PositionId = signature.PositionId;
            Role = signature.Role;
            Sign = signature.Sign;
            RequestedSigner = signature.RequestedSigner;
        }

        public Guid Id { get; private set; }
        public Guid DatabaseId { get; private set; }
        public int PositionId { get; private set; }
        public string Role { get; private set; }
        public string Sign { get; private set; }
        public string RequestedSigner { get; private set; }
    }

    internal class PersonWrapper
    {
        public PersonWrapper(IPerson creator)
        {
            if (creator == null) 
                return;

            Id = creator.Id;
            Login = creator.Login;
            DisplayName = creator.DisplayName;
            Positions = new ReadOnlyCollection<PositionWrapper>(creator.Positions.Select(p => new PositionWrapper(p)).ToList());
            MainPosition = new PositionWrapper(creator.MainPosition);
            Comment = creator.Comment;
            Sid = creator.Sid;
            IsDeleted = creator.IsDeleted;
            IsAdmin = creator.IsAdmin;
            ActualName = creator.ActualName;
        }

        public int Id { get; private set; }
        public string Login { get; private set; }
        public string DisplayName { get; private set; }
        public ReadOnlyCollection<PositionWrapper> Positions { get; private set; }
        public PositionWrapper MainPosition { get; private set; }
        public string Comment { get; private set; }
        public string Sid { get; private set; }
        public bool IsDeleted { get; private set; }
        public bool IsAdmin { get; private set; }
        public string ActualName { get; private set; }
    }

    internal class PositionWrapper
    {
        public PositionWrapper(IPosition position)
        {
            if (position == null) 
                return;

            Order = position.Order;
            Position = position.Position;
        }

        public int Order { get; private set; }
        public int Position { get; private set; }
    }

    internal class TypeWrapper
    {
        public TypeWrapper(IType type)
        {
            if (type == null) 
                return;

            Id = type.Id;
            Name = type.Name;
            Title = type.Title;
            Sort = type.Sort;
            HasFiles = type.HasFiles;
            Children = new ReadOnlyCollection<int>(type.Children);
            Attributes = new ReadOnlyCollection<AttributeWrapper>(type.Attributes.Select(x => new AttributeWrapper(x)).ToList());
            DisplayAttributes = new List<AttributeWrapper>(type.DisplayAttributes.Select(x => new AttributeWrapper(x)));
            SvgIcon = type.SvgIcon;
            IsMountable = type.IsMountable;
            IsDeleted = type.IsDeleted;
            Kind = type.Kind;
            IsService = type.IsService;
        }

        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Title { get; private set; }
        public int Sort { get; private set; }
        public bool HasFiles { get; private set; }
        public ReadOnlyCollection<int> Children { get; private set; }
        public ReadOnlyCollection<AttributeWrapper> Attributes { get; private set; }
        public IEnumerable<AttributeWrapper> DisplayAttributes { get; private set; }
        public byte[] SvgIcon { get; private set; }
        public bool IsMountable { get; private set; }
        public TypeKind Kind { get; private set; }
        public bool IsDeleted { get; private set; }
        public bool IsService { get; private set; }
    }

    internal class AttributeWrapper
    {
        public string Name { get; private set; }
        public string Title { get; private set; }
        public bool IsObligatory { get; private set; }
        public int DisplaySortOrder { get; private set; }
        public bool ShowInObjectsExplorer { get; private set; }
        public bool IsService { get; private set; }
        public string Configuration { get; private set; }
        public int DisplayHeight { get; private set; }

        public AttributeWrapper(IAttribute attribute)
        {
            Name = attribute.Name;
            Title = attribute.Title;
            IsObligatory = attribute.IsObligatory;
            DisplaySortOrder = attribute.DisplaySortOrder;
            ShowInObjectsExplorer = attribute.ShowInObjectsExplorer;
            IsService = attribute.IsService;
            Configuration = attribute.Configuration;
            DisplayHeight = attribute.DisplayHeight;
        }
    }

    internal class OrganisationUnitWrapper
    {
        public OrganisationUnitWrapper(IOrganisationUnit organisationUnit)
        {
            if (organisationUnit == null) 
                return;

            Id = organisationUnit.Id;
            Title = organisationUnit.Title;
            IsDeleted = organisationUnit.IsDeleted;
            Children = new ReadOnlyCollection<int>(organisationUnit.Children);
        }

        public int Id { get; private set; }
        public string Title { get; private set; }
        public bool IsDeleted { get; private set; }
        public ReadOnlyCollection<int> Children { get; private set; }
    }


    internal class AccessWrapper
    {
        public AccessWrapper(IAccessRecord accessRecord)
        {
            if (accessRecord == null)
                return;

            AccessLevel = accessRecord.Access.AccessLevel;
            ValidThrough = accessRecord.Access.ValidThrough;
            IsInheritable = accessRecord.Access.IsInheritable;
            IsInherited = accessRecord.Access.IsInherited;
        }
        public AccessLevel AccessLevel { get; private set; }
        public DateTime ValidThrough { get; private set; }
        public bool IsInheritable { get; private set; }
        public bool IsInherited { get; private set; }
    }
}
