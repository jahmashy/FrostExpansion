using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Shared.Models.Enums
{
    public enum FileStatusCode
    {
        FileDoesNotExists,
        NotEnoughFiles,
        TooManyFiles,
        FileIsTooBig,
        FileIsNull,
        InvalidFileExtension,
        Success
    }
    public enum ChatStatusCode
    {
        Success,
        ChatDoesNotExists,
        ChatIsNotGroupChat,
        UserIsAlreadyInChat,
        UserIsNotInChat,
        UserDoesNotExists,
        CommunicationIsBlocked,
        MaxParticipantsReached,
        InvitationIsNotValid,
        InvitationHasExpired
    }
    public enum PropertyStatusCode
    {
        Success,
        PropertyDoesNotExists,
        UserDoesNotExists,
        PropertyIsAlreadyFollowed,
        PropertyIsNotFollowed,
        PropertyDoesNotAllowRoommates,
        UserIsAlreadyRoommate,
        UserIsNotRoommate,
        TemplateIsAlreadySaved,
        TemplateDoesNotExist,
        UserIsNotOwner
    }
}
