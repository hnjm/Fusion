namespace ActualLab.Rpc;

public record RpcPeerRef(Symbol Key, bool IsServer = false, bool IsBackend = false)
{
    public static RpcPeerRef Default { get; set; } = NewClient("default");

    public static RpcPeerRef NewServer(Symbol key, bool isBackend = false)
        => new(key, true, isBackend);
    public static RpcPeerRef NewClient(Symbol key, bool isBackend = false)
        => new(key, false, isBackend);

    public bool CanBecomeObsolete { get; } = false;
    public virtual bool IsObsolete => false;

    public override string ToString()
        => $"{(IsBackend ? "backend-" : "")}{(IsServer ? "server" : "client")}:{Key}";

    public virtual VersionSet GetVersions()
        => IsBackend ? RpcDefaults.BackendPeerVersions : RpcDefaults.ApiPeerVersions;

    // Operators

    public static implicit operator RpcPeerRef(RpcPeer peer) => peer.Ref;
}
