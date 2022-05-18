﻿DROP table [b2b].[Reasons]

CREATE TABLE [b2b].[ReasonTranslations](
    [Id] [uniqueidentifier] NOT NULL,
    [RecordId] [int] IDENTITY(1,1) NOT NULL,
    [ErrorCode] [nvarchar](250) NOT NULL,
    [Code] [nvarchar](3) NOT NULL,
    [Text] [nvarchar](max) NOT NULL,
    [LanguageCode] [nvarchar](2) NOT NULL
    CONSTRAINT [PK_ReasonTranslations] PRIMARY KEY NONCLUSTERED
    CONSTRAINT [UC_Code] UNIQUE (ErrorCode, LanguageCode)
(
[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
    CONSTRAINT [UC_ReasonTranslations_Id] UNIQUE CLUSTERED
(
[RecordId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]