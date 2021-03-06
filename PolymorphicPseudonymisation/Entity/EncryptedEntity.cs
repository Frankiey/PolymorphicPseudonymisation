﻿using System;
using PolymorphicPseudonymisation.Exceptions;
using PolymorphicPseudonymisation.Key;
using PolymorphicPseudonymisation.Parser;

namespace PolymorphicPseudonymisation.Entity
{
    public abstract class EncryptedEntity : Identifiable
    {
        public static T FromBase64<T>(string base64, EncryptedVerifiers verifiers) where T : EncryptedEntity
        {
            var key = FromBase64(base64, verifiers);
            if (!(key is T))
            {
                throw new PolymorphicPseudonymisationException(
                    $"Expected instance of {typeof(T).Name}, got {key.GetType().Name}");
            }

            return (T) key;
        }

        protected EncryptedEntity(EncryptedEntityParser parser)
        {
            SchemeVersion = parser.SchemeVersion;
            SchemeKeyVersion = parser.SchemeKeyVersion;
            Recipient = parser.Recipient;
            RecipientKeySetVersion = parser.RecipientKeySetVersion;
        }

        private static EncryptedEntity FromBase64(string base64, EncryptedVerifiers verifiers)
        {
            var encoded = Convert.FromBase64String(base64);
            var parser = new EncryptedEntityParser(encoded);
            parser.Decode(verifiers);
            switch (parser.BsnkType.ObjectIdentifier)
            {
                case BsnkType.EncryptedIdentityName:
                case BsnkType.SignedEncryptedIdentityName:
                    return new EncryptedIdentity(parser);
                case BsnkType.EncryptedPseudonymName:
                case BsnkType.SignedEncryptedPseudonymName:
                    return new EncryptedPseudonym(parser);
                default:
                    throw new PolymorphicPseudonymisationException($"Unexpected type {parser.BsnkType}");
            }
        }
    }
}