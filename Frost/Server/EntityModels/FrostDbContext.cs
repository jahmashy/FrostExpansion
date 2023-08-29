using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Frost.Server.EntityModels;

public partial class FrostDbContext : DbContext
{
    public FrostDbContext()
    {
    }
    private IConfiguration _configuration;
    public FrostDbContext(DbContextOptions<FrostDbContext> options, IConfiguration configuration)
            : base(options)
    {
        _configuration = configuration;
    }


    public virtual DbSet<AdministrativeArea> AdministrativeAreas { get; set; }

    public virtual DbSet<BlockedCommunication> BlockedCommunications { get; set; }

    public virtual DbSet<Chat> Chats { get; set; }

    public virtual DbSet<ChatInvitation> ChatInvitations { get; set; }

    public virtual DbSet<ChatRoom> ChatRooms { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<FollowedOffer> FollowedOffers { get; set; }

    public virtual DbSet<MarketType> MarketTypes { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<OfferType> OfferTypes { get; set; }

    public virtual DbSet<Property> Properties { get; set; }

    public virtual DbSet<PropertyRoommate> PropertyRoommates { get; set; }

    public virtual DbSet<PropertyType> PropertyTypes { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SavedFilter> SavedFilters { get; set; }

    public virtual DbSet<Sublocality> Sublocalities { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<ViewCount> ViewCounts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer(_configuration["ConnectionString"]);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdministrativeArea>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Administrative_Area_pk");

            entity.ToTable("Administrative_Area");

            entity.HasIndex(e => e.GoogleId, "AD_GoogleID").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.GoogleId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("GoogleID");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<BlockedCommunication>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Blocked_Communication_pk");

            entity.ToTable("Blocked_Communication");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.User1Id).HasColumnName("User_1_ID");
            entity.Property(e => e.User2Id).HasColumnName("User_2_ID");

            entity.HasOne(d => d.User1).WithMany(p => p.BlockedCommunicationUser1s)
                .HasForeignKey(d => d.User1Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Blocked_Communication_User1");

            entity.HasOne(d => d.User2).WithMany(p => p.BlockedCommunicationUser2s)
                .HasForeignKey(d => d.User2Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Blocked_Communication_User2");
        });

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Chat_pk");

            entity.ToTable("Chat");

            entity.Property(e => e.Id).HasColumnName("ID");
        });

        modelBuilder.Entity<ChatInvitation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Chat_Invitation_pk");

            entity.ToTable("Chat_Invitation");

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .IsUnicode(false)
                .HasColumnName("ID");
            entity.Property(e => e.ChatId).HasColumnName("Chat_ID");
            entity.Property(e => e.ExpirationDate)
                .HasColumnType("datetime")
                .HasColumnName("Expiration_Date");

            entity.HasOne(d => d.Chat).WithMany(p => p.ChatInvitations)
                .HasForeignKey(d => d.ChatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Chat_Invitation_Chat");
        });

        modelBuilder.Entity<ChatRoom>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ChatRoom_pk");

            entity.ToTable("ChatRoom");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ChatId).HasColumnName("Chat_ID");
            entity.Property(e => e.UserId).HasColumnName("User_ID");

            entity.HasOne(d => d.Chat).WithMany(p => p.ChatRooms)
                .HasForeignKey(d => d.ChatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ChatRoom_Chat");

            entity.HasOne(d => d.User).WithMany(p => p.ChatRooms)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ChatRoom_User");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("City_pk");

            entity.ToTable("City");

            entity.HasIndex(e => e.GoogleId, "C_GoogleID").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.GoogleId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("GoogleID");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<FollowedOffer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Followed_Offer_pk");

            entity.ToTable("Followed_Offer");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.PropertyId).HasColumnName("Property_ID");
            entity.Property(e => e.UserId).HasColumnName("User_ID");

            entity.HasOne(d => d.Property).WithMany(p => p.FollowedOffers)
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SavedOffer_Property");

            entity.HasOne(d => d.User).WithMany(p => p.FollowedOffers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("SavedOffer_User");
        });

        modelBuilder.Entity<MarketType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Market_Type_pk");

            entity.ToTable("Market_Type");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Message_pk");

            entity.ToTable("Message");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ChatId).HasColumnName("Chat_ID");
            entity.Property(e => e.MessageContent)
                .HasMaxLength(4096)
                .IsUnicode(false);
            entity.Property(e => e.MessageDate).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("User_ID");

            entity.HasOne(d => d.Chat).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ChatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Message_Chat");

            entity.HasOne(d => d.User).WithMany(p => p.Messages)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Message_User");
        });

        modelBuilder.Entity<OfferType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Offer_Type_pk");

            entity.ToTable("Offer_Type");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Property>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Property_pk");

            entity.ToTable("Property");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AdministrativeAreaId).HasColumnName("Administrative_Area_ID");
            entity.Property(e => e.CityId).HasColumnName("City_ID");
            entity.Property(e => e.CreationDate).HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(2000)
                .IsUnicode(false);
            entity.Property(e => e.ExpirationDate).HasColumnType("datetime");
            entity.Property(e => e.IsTemplate).HasColumnName("isTemplate");
            entity.Property(e => e.MarketTypeId).HasColumnName("Market_Type_ID");
            entity.Property(e => e.OfferTypeId).HasColumnName("Offer_Type_ID");
            entity.Property(e => e.PropertyTypeId).HasColumnName("Property_Type_ID");
            entity.Property(e => e.SublocalityId).HasColumnName("Sublocality_ID");
            entity.Property(e => e.Title)
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.UserId).HasColumnName("User_ID");

            entity.HasOne(d => d.AdministrativeArea).WithMany(p => p.Properties)
                .HasForeignKey(d => d.AdministrativeAreaId)
                .HasConstraintName("Property_Administrative_Area");

            entity.HasOne(d => d.City).WithMany(p => p.Properties)
                .HasForeignKey(d => d.CityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Property_City");

            entity.HasOne(d => d.MarketType).WithMany(p => p.Properties)
                .HasForeignKey(d => d.MarketTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Property_Market_Type");

            entity.HasOne(d => d.OfferType).WithMany(p => p.Properties)
                .HasForeignKey(d => d.OfferTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Property_Offer_Type");

            entity.HasOne(d => d.PropertyType).WithMany(p => p.Properties)
                .HasForeignKey(d => d.PropertyTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Property_Property_Type");

            entity.HasOne(d => d.Sublocality).WithMany(p => p.Properties)
                .HasForeignKey(d => d.SublocalityId)
                .HasConstraintName("Property_Sublocality");

            entity.HasOne(d => d.User).WithMany(p => p.Properties)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Property_User");
        });

        modelBuilder.Entity<PropertyRoommate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Property_Roommates_pk");

            entity.ToTable("Property_Roommates");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.PropertyId).HasColumnName("Property_ID");
            entity.Property(e => e.UserId).HasColumnName("User_ID");

            entity.HasOne(d => d.Property).WithMany(p => p.PropertyRoommates)
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Property_Roommates_Property");

            entity.HasOne(d => d.User).WithMany(p => p.PropertyRoommates)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Property_Roommates_User");
        });

        modelBuilder.Entity<PropertyType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Property_Type_pk");

            entity.ToTable("Property_Type");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Role_pk");

            entity.ToTable("Role");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.RoleName)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<SavedFilter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Saved_Filters_pk");

            entity.ToTable("Saved_Filters");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CityId).HasColumnName("City_ID");
            entity.Property(e => e.MarketTypeId).HasColumnName("Market_Type_ID");
            entity.Property(e => e.OfferTypeId).HasColumnName("Offer_Type_ID");
            entity.Property(e => e.PropertyTypeId).HasColumnName("Property_Type_ID");

            entity.HasOne(d => d.City).WithMany(p => p.SavedFilters)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("Saved_Filters_City");

            entity.HasOne(d => d.MarketType).WithMany(p => p.SavedFilters)
                .HasForeignKey(d => d.MarketTypeId)
                .HasConstraintName("Saved_Filters_Market_Type");

            entity.HasOne(d => d.OfferType).WithMany(p => p.SavedFilters)
                .HasForeignKey(d => d.OfferTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Saved_Filters_Offer_Type");

            entity.HasOne(d => d.PropertyType).WithMany(p => p.SavedFilters)
                .HasForeignKey(d => d.PropertyTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Saved_Filters_Property_Type");
        });

        modelBuilder.Entity<Sublocality>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Sublocality_pk");

            entity.ToTable("Sublocality");

            entity.HasIndex(e => e.GoogleId, "S_GoogleID").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.GoogleId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("GoogleID");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("User_pk");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "Email").IsUnique();

            entity.HasIndex(e => e.TelNumber, "TelNumber").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CityId).HasColumnName("City_ID");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Nationality)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Password).IsUnicode(false);
            entity.Property(e => e.PasswordSalt).IsUnicode(false);
            entity.Property(e => e.RefreshToken)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.RefreshTokenExpirationDate).HasColumnType("datetime");
            entity.Property(e => e.RoleId).HasColumnName("Role_ID");
            entity.Property(e => e.SavedFiltersId).HasColumnName("Saved_Filters_ID");

            entity.HasOne(d => d.City).WithMany(p => p.Users)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("User_City");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("User_Role");

            entity.HasOne(d => d.SavedFilters).WithMany(p => p.Users)
                .HasForeignKey(d => d.SavedFiltersId)
                .HasConstraintName("User_Saved_Filters");
        });

        modelBuilder.Entity<ViewCount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("View_Count_pk");

            entity.ToTable("View_Count");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.PropertyId).HasColumnName("Property_ID");
            entity.Property(e => e.UserId).HasColumnName("User_ID");

            entity.HasOne(d => d.Property).WithMany(p => p.ViewCounts)
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("View_Count_Property");

            entity.HasOne(d => d.User).WithMany(p => p.ViewCounts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("View_Count_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
