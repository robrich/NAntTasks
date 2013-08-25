CREATE TABLE [dbo].[UserSecurity](
	[UserSecurityId] [int] IDENTITY(1,1) NOT NULL,
	[SecurityId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	CONSTRAINT [PK_UserSecurity] PRIMARY KEY CLUSTERED (
		[UserSecurityId] ASC
	)
)

/* DOWN
DROP TABLE [dbo].[UserSecurity]
*/
