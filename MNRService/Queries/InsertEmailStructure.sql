INSERT INTO dputEmailStructure(
    Sender,
    Receiver,
    Subject,
    Body,
    FilePath,
    SendDate,
    Status,
    IsActive,
    CrtByTokenID,
    CrtDate,
    SenderPassword,
    CommunicationTypeID,
    KeyPath,
    HostKeyFingerprint
)
VALUES (
    @Sender,
    @Receiver,
    @Subject,
    @Body,
    @FilePath,
    NOW(), -- MySQL equivalent of GETDATE()
    0,     -- Status (Pending)
    1,     -- IsActive (True)
    1,     -- CrtByTokenID (Assuming 1)
    NOW(), -- CrtDate
    @SenderPassword,
    @CommunicationTypeID,
    @KeyPath,
    @HostKeyFingerprint
);
SELECT 'Insert successful.' AS Message;