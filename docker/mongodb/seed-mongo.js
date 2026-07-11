// MongoDB Seed Data for AutoPulse
// To execute this seed script, run the following command (or run it within MongoDB Compass / mongosh):
// mongosh "mongodb://localhost:27017/autopulse" seed-mongo.js

db = db.getSiblingDB('autopulse');

// Clean up existing documents to avoid duplicate keys
db.VehicleSpecificationDocument.deleteMany({});

db.VehicleSpecificationDocument.insertMany([
  {
    _id: "019056d0-2a81-7000-8b1e-50e50f3c5be0",
    AuctionId: "019056d0-2a81-7000-8b1e-50e50f3c5be0",
    Brand: "Honda",
    Model: "Accord",
    DynamicMetadata: {
      engine: "1.5L I4 Turbocharged",
      transmission: "CVT",
      color: "Modern Steel Metallic",
      doors: 4,
      fuelType: "Gasoline",
      features: ["Sunroof", "Apple CarPlay", "Adaptive Cruise Control", "Lane Keeping Assist"],
      safetyRating: "5-Star"
    },
    CreatedAt: new Date(),
    UpdatedAt: null,
    DeletedAt: null
  },
  {
    _id: "019056d0-2a81-7000-8b1e-50e50f3c5be1",
    AuctionId: "019056d0-2a81-7000-8b1e-50e50f3c5be1",
    Brand: "Toyota",
    Model: "Camry",
    DynamicMetadata: {
      engine: "2.5L 4-Cylinder",
      transmission: "8-Speed Automatic",
      color: "Super White",
      doors: 4,
      fuelType: "Hybrid",
      features: ["Heated Seats", "Blind Spot Monitor", "Back-up Camera"],
      batteryWarranty: "10 years / 150,000 miles"
    },
    CreatedAt: new Date(),
    UpdatedAt: null,
    DeletedAt: null
  },
  {
    _id: "019056d0-2a81-7000-8b1e-50e50f3c5be2",
    AuctionId: "019056d0-2a81-7000-8b1e-50e50f3c5be2",
    Brand: "Land Rover",
    Model: "Defender",
    DynamicMetadata: {
      engine: "3.0L P400 MHEV",
      transmission: "8-Speed Automatic",
      color: "Pangea Green",
      doors: 5,
      fuelType: "Mild Hybrid",
      features: ["AWD", "Air Suspension", "3D Surround Camera", "Off-Road Pack"],
      towingCapacityLbs: 8200
    },
    CreatedAt: new Date(),
    UpdatedAt: null,
    DeletedAt: null
  },
  {
    _id: "019056d0-2a81-7000-8b1e-50e50f3c5be3",
    AuctionId: "019056d0-2a81-7000-8b1e-50e50f3c5be3",
    Brand: "Ford",
    Model: "Mustang GT",
    DynamicMetadata: {
      engine: "5.0L V8",
      transmission: "6-Speed Manual",
      color: "Shadow Black",
      doors: 2,
      fuelType: "Gasoline",
      features: ["Active Valve Performance Exhaust", "Brembo Brakes", "Track Apps", "Leather Seats"],
      horsePower: 460
    },
    CreatedAt: new Date(),
    UpdatedAt: null,
    DeletedAt: null
  }
]);

print("MongoDB seed completed successfully!");
