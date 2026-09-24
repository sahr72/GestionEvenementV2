using EventManagement.Domain.Entities;
using System.Text;
using System.Xml.Linq;

namespace EventManagement.Application.Interfaces;
/*
 * Spécialisé par Aggregate ( Evenement), pas générique.
   GetByIdAsyncpeut être utilisé pour charger un agrégat en mode modification.
   SaveChangesAsyncest exposé (pas de wrapper UnitOfWork artificiel).
 */
public interface ICommentaireRepository
{
 /*  Commentaire est une entité enfant de l'agrégat Evenement. Seule la racine d'agrégat(Event) possède un référentiel.
 En DDD tactique, le référentiel est défini par agrégat , pas par entité.
 Pour modifier un commentaire, vous chargez l' Evenement via IEventRepository, 
 vous appelez une méthode du domaine ( AddCommentaire, , ), puis vous sauvegardez l'agrégat entier.
 */
}